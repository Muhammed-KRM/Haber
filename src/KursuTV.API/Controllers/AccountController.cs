using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Context;
using Microsoft.EntityFrameworkCore;
using KursuTV.Data.Enums;
using System.Text.Json;

namespace KursuTV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// KVK KapsamÄ±nda kullanÄ±cÄ±nÄ±n tÃ¼m kiÅŸisel verilerini JSON formatÄ±nda dÄ±ÅŸa aktarÄ±r (Data Export).
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportData()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

        var user = await _context.Users
            .Include(u => u.Listings)
            .Include(u => u.SentMessages)
            .Include(u => u.ReceivedMessages)
            .Include(u => u.TokenTransactions)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound();

        var exportData = new
        {
            PersonalInfo = new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.CreatedAt,
                user.TokenBalance
            },
            Listings = user.Listings.Select(l => new { l.Id, l.Title, l.Status, l.CreatedAt }),
            Messages = new
            {
                Sent = user.SentMessages.Select(m => new { m.Id, m.CreatedAt, m.Status }),
                Received = user.ReceivedMessages.Select(m => new { m.Id, m.CreatedAt, m.Status })
            },
            TokenHistory = user.TokenTransactions.Select(t => new { t.Id, t.Amount, t.Type, t.CreatedAt })
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(exportData, options);

        return File(jsonBytes, "application/json", $"kursutv_export_{DateTime.UtcNow:yyyyMMdd}.json");
    }

    /// <summary>
    /// Mobil uygulama iÃ§in Firebase Cloud Messaging token'Ä±nÄ± kaydeder.
    /// </summary>
    [HttpPost("fcm-token")]
    public async Task<IActionResult> SaveFcmToken([FromBody] FcmTokenDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.FcmToken = dto.Token;
        await _context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// KVK KapsamÄ±nda hesabÄ± kalÄ±cÄ± silmez, yasal saklama sÃ¼releri nedeniyle "Soft Delete" (AskÄ±ya Alma) uygular.
    /// 6 ay sonra anonimleÅŸtirilecek.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> DeleteAccount()    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

        var user = await _context.Users
            .Include(u => u.Listings)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound();

        // 1. Hesap PasifleÅŸtirilir
        user.IsActive = false;

        // 2. TÃ¼m aÃ§Ä±k ilanlarÄ± kapatÄ±lÄ±r
        foreach (var listing in user.Listings)
        {
            listing.Status = ListingStatus.Closed;
        }

        // 3. Ä°leride sistemde kalÄ±cÄ± silme CronJob'u tarafÄ±ndan iÅŸaretlenmesi iÃ§in tarih konulabilir
        // user.DeletedAt = DateTime.UtcNow; // (Soft-Delete implementation details vary, we simply use IsActive=false)

        await _context.SaveChangesAsync();

        return Ok(new { Message = "HesabÄ±nÄ±z baÅŸarÄ±yla dondurulmuÅŸtur. KVK politikamÄ±z gereÄŸi verileriniz yasal sÃ¼re bitiminde tamemen anonimleÅŸtirilecektir." });
    }

    /// <summary>
    /// Mevcut kullanÄ±cÄ±nÄ±n temel bilgilerini dÃ¶ndÃ¼rÃ¼r (ban durumu, ihlal sayÄ±sÄ± vb.).
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.ViolationCount,
            user.BannedUntil,
            user.BanReason,
            user.FcmToken
        });
    }

    /// <summary>
    /// KullanÄ±cÄ±nÄ±n son aktivitelerini dÃ¶ndÃ¼rÃ¼r (mesajlar, ilanlar, yorumlar).
    /// </summary>
    [HttpGet("activities")]
    public async Task<IActionResult> GetActivities()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

        var activities = new List<object>();

        // Son mesajlar
        var recentMessages = await _context.Messages
            .Where(m => m.ReceiverId == userId && m.CreatedAt > DateTime.UtcNow.AddDays(-30))
            .OrderByDescending(m => m.CreatedAt)
            .Take(5)
            .Select(m => new { m.CreatedAt, Type = "message" })
            .ToListAsync();

        foreach (var msg in recentMessages)
            activities.Add(new { Icon = "ğŸ’¬", Description = "Yeni mesaj aldÄ±nÄ±z", msg.CreatedAt });

        // Son ilanlar
        var recentListings = await _context.Listings
            .Where(l => l.OwnerId == userId && l.CreatedAt > DateTime.UtcNow.AddDays(-30))
            .OrderByDescending(l => l.CreatedAt)
            .Take(3)
            .Select(l => new { l.Title, l.CreatedAt, l.Status })
            .ToListAsync();

        foreach (var listing in recentListings)
            activities.Add(new { Icon = "ğŸ“‹", Description = $"\"{listing.Title}\" ilanÄ±nÄ±z {(listing.Status.ToString() == "Active" ? "yayÄ±nda" : "oluÅŸturuldu")}", listing.CreatedAt });

        // Son yorumlar
        var recentReviews = await _context.Reviews
            .Where(r => r.ReviewedId == userId && r.CreatedAt > DateTime.UtcNow.AddDays(-30))
            .OrderByDescending(r => r.CreatedAt)
            .Take(3)
            .Select(r => new { r.AverageRating, r.CreatedAt })
            .ToListAsync();

        foreach (var review in recentReviews)
            activities.Add(new { Icon = "â­", Description = $"Yeni deÄŸerlendirme aldÄ±nÄ±z ({review.AverageRating:F1} puan)", review.CreatedAt });

        // Token iÅŸlemleri
        var recentTokens = await _context.TokenTransactions
            .Where(t => t.UserId == userId && t.CreatedAt > DateTime.UtcNow.AddDays(-30))
            .OrderByDescending(t => t.CreatedAt)
            .Take(3)
            .Select(t => new { t.Amount, t.Type, t.CreatedAt })
            .ToListAsync();

        foreach (var token in recentTokens)
            activities.Add(new { Icon = "ğŸ’°", Description = $"{(token.Type.ToString() == "Purchase" ? "Jeton satÄ±n aldÄ±nÄ±z" : "Jeton harcandÄ±")} ({token.Amount} jeton)", token.CreatedAt });

        var sorted = activities
            .OrderByDescending(a => (DateTime)a.GetType().GetProperty("CreatedAt")!.GetValue(a)!)
            .Take(10)
            .ToList();

        return Ok(sorted);
    }
}

public record FcmTokenDto(string Token);
