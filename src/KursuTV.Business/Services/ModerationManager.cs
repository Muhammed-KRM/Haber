using MassTransit;
using System.Text.RegularExpressions;
using KursuTV.Business.Events;
using KursuTV.Business.Infrastructure.Moderation;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Entities;
using KursuTV.Data.Repositories;

namespace KursuTV.Business.Services;

public class ModerationManager : IModerationService
{
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // HATA KODLARI â€” ModerationManager (Prefix: MM)
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    private const string EC_CHECK  = "MM-001"; // CheckContent
    private const string EC_STRIKE = "MM-002"; // AddStrikeAsync
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    private readonly IRepository<User> _userRepo;
    private readonly IRepository<ViolationLog> _violationRepo;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogService _logService;

    // Compiled regex'ler â€” static, bir kez derlenir

    // TÃ¼rk mobil: 05XX ile baÅŸlayan, tÃ¼m ayraÃ§ varyasyonlarÄ± (boÅŸluk, tire, nokta vb.)
    private static readonly Regex PhoneRegex = new(
        @"(\+90|0090|0)[\s\-\.\(\)\*\[\]\/\\]?[5][0-9][\s\-\.\(\)\*\[\]\/\\]?\d{2}[\s\-\.\(\)\*\[\]\/\\]?\d{3}[\s\-\.\(\)\*\[\]\/\\]?\d{2}[\s\-\.\(\)\*\[\]\/\\]?\d{2}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // BoÅŸluksuz 11 haneli (05XXXXXXXXX)
    private static readonly Regex PhoneSimpleRegex = new(
        @"\b0[5][0-9]\d{8}\b",
        RegexOptions.Compiled);

    // BoÅŸluklu/ayraÃ§lÄ± format: 0505 050 5838, 0505-050-5838 vb.
    private static readonly Regex PhoneSpacedRegex = new(
        @"\b0[5][0-9]\d[\s\-\.\(\)]\d{3}[\s\-\.\(\)]\d{2}[\s\-\.\(\)]\d{2}\b",
        RegexOptions.Compiled);

    // 5XX ile baÅŸlayan (baÅŸta 0 yok): 505 050 5838
    private static readonly Regex PhoneNoLeadingZeroRegex = new(
        @"\b[5][0-9]\d[\s\-\.\(\)]?\d{3}[\s\-\.\(\)]?\d{2}[\s\-\.\(\)]?\d{2}\b",
        RegexOptions.Compiled);

    private static readonly Regex EmailRegex = new(
        @"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex LinkRegex = new(
        @"(https?://[^\s]+|\bwww\.[a-zA-Z0-9\-]+\.[a-zA-Z]{2,})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ModerationManager(
        IRepository<User> userRepo,
        IRepository<ViolationLog> violationRepo,
        IPublishEndpoint publishEndpoint,
        ILogService logService)
    {
        _userRepo = userRepo;
        _violationRepo = violationRepo;
        _publishEndpoint = publishEndpoint;
        _logService = logService;
    }

    public ModerationResult CheckContent(string title, string description)
    {
        // Normalize et (homoglyph + TÃ¼rkÃ§e rakam)
        var raw = $"{title} {description}";
        var normalized = TurkishTextNormalizer.Normalize(raw);

        if (PhoneRegex.IsMatch(normalized) || PhoneSimpleRegex.IsMatch(normalized)
            || PhoneSpacedRegex.IsMatch(normalized) || PhoneNoLeadingZeroRegex.IsMatch(normalized))
            return ModerationResult.Violation("Phone",
                "Ä°lan iÃ§eriÄŸinde telefon numarasÄ± paylaÅŸÄ±lamaz. Platform Ã¼zerinden iletiÅŸim kurulmalÄ±dÄ±r.");

        if (EmailRegex.IsMatch(normalized))
            return ModerationResult.Violation("Email",
                "Ä°lan iÃ§eriÄŸinde e-posta adresi paylaÅŸÄ±lamaz.");

        if (LinkRegex.IsMatch(normalized))
            return ModerationResult.Violation("Link",
                "Ä°lan iÃ§eriÄŸinde harici link paylaÅŸÄ±lamaz.");

        return ModerationResult.Clean();
    }

    public async Task AddStrikeAsync(Guid userId, Guid? listingId, string listingTitle,
        string violationType, string detectedContent, string detectedBy)
    {
        try
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return;

            user.ViolationCount++;
            user.LastViolationAt = DateTime.UtcNow;

            // Ban hesapla
            var ban = CalculateBan(user.ViolationCount);
            if (ban.HasValue)
            {
                user.BannedUntil = ban == TimeSpan.MaxValue
                    ? DateTime.MaxValue
                    : DateTime.UtcNow.Add(ban.Value);
                user.BanReason = $"Otomatik: {violationType} ihlali ({user.ViolationCount}. ihlal)";
            }

            _userRepo.Update(user);

            // Ä°hlal kaydÄ±
            await _violationRepo.AddAsync(new ViolationLog
            {
                UserId = userId,
                ListingId = listingId,
                ListingTitle = listingTitle,
                ViolationType = violationType,
                DetectedContent = detectedContent.Length > 200
                    ? detectedContent[..200] + "..." : detectedContent,
                DetectedBy = detectedBy,
                CreatedAt = DateTime.UtcNow
            });

            await _userRepo.SaveChangesAsync();

            // Bildirim gÃ¶nder â€” ban mÄ± uyarÄ± mÄ±?
            var isBanned = ban.HasValue;
            var notifType = isBanned ? "Ban" : "Warning";
            var notifTitle = isBanned
                ? (ban == TimeSpan.MaxValue ? "HesabÄ±nÄ±z KalÄ±cÄ± Olarak AskÄ±ya AlÄ±ndÄ± ğŸš«" : "HesabÄ±nÄ±z GeÃ§ici Olarak AskÄ±ya AlÄ±ndÄ± ğŸš«")
                : "Ä°lan Ä°Ã§eriÄŸi UyarÄ±sÄ± âš ï¸";
            var notifMessage = isBanned
                ? (ban == TimeSpan.MaxValue
                    ? $"HesabÄ±nÄ±z kalÄ±cÄ± olarak askÄ±ya alÄ±ndÄ±. Sebep: {violationType} ihlali ({user.ViolationCount}. ihlal)."
                    : $"HesabÄ±nÄ±z {user.BannedUntil:dd.MM.yyyy} tarihine kadar askÄ±ya alÄ±ndÄ±. Sebep: {violationType} ihlali ({user.ViolationCount}. ihlal).")
                : $"Ä°lan iÃ§eriÄŸinizde kural ihlali tespit edildi ({violationType}). Toplam ihlal: {user.ViolationCount}. Tekrar eden ihlallerde hesabÄ±nÄ±z askÄ±ya alÄ±nabilir.";

            // Bildirim gÃ¶nder â€” await ile, hata loglanÄ±r
            try
            {
                await _publishEndpoint.Publish(new SendNotificationEvent
                {
                    UserId = userId,
                    Type = notifType,
                    Title = notifTitle,
                    Message = notifMessage,
                    ActionUrl = "/panel/ilanlarim",
                    SendEmail = true,
                    UserEmail = user.Email,
                    IdempotencyKey = $"strike-{userId}-{user.ViolationCount}"
                });
            }
            catch (Exception ex)
            {
                await _logService.LogFunctionErrorAsync("MM-NOTIF", ex, new { userId, violationType });
            }
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_STRIKE, ex, new { userId, violationType });
            throw;
        }
    }

    private static TimeSpan? CalculateBan(int count) => count switch
    {
        >= 11 => TimeSpan.MaxValue,
        8     => TimeSpan.FromDays(30),
        5     => TimeSpan.FromDays(7),
        _     => null
    };
}
