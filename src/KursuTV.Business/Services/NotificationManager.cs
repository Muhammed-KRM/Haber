using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Entities;
using KursuTV.Data.Repositories;

namespace KursuTV.Business.Services;

public class NotificationManager : INotificationService
{
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // HATA KODLARI â€” NotificationManager (Prefix: NM)
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    private const string EC_CREATE   = "NM-001";
    private const string EC_GETCOUNT = "NM-002";
    private const string EC_GETLIST  = "NM-003";
    private const string EC_READ     = "NM-004";
    private const string EC_READALL  = "NM-005";
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    private readonly IRepository<Notification> _repo;
    private readonly ILogService _logService;
    private readonly ISmsService _smsService;
    private readonly KursuTV.Business.Infrastructure.Messaging.IFcmService _fcmService;

    public NotificationManager(IRepository<Notification> repo, ILogService logService, ISmsService smsService, KursuTV.Business.Infrastructure.Messaging.IFcmService fcmService)
    {
        _repo = repo;
        _logService = logService;
        _smsService = smsService;
        _fcmService = fcmService;
    }

    public async Task<Notification> CreateAsync(Guid userId, string type, string title,
        string message, string? actionUrl = null, string? idempotencyKey = null)
    {
        try
        {
            // Duplicate Ã¶nleme: aynÄ± idempotency key varsa tekrar yazma
            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                var existing = await _repo.FindAsync(n =>
                    n.UserId == userId &&
                    n.Type == type &&
                    n.IdempotencyKey == idempotencyKey);
                if (existing.Any()) return existing.First();
            }

            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                ActionUrl = actionUrl,
                IdempotencyKey = idempotencyKey,
                ExpiresAt = DateTime.UtcNow.AddDays(90)
            };
            await _repo.AddAsync(notification);
            await _repo.SaveChangesAsync();
            return notification;
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_CREATE, ex, new { userId, type });
            throw;
        }
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        try
        {
            var all = await _repo.FindAsync(n => n.UserId == userId && !n.IsRead);
            return all.Count();
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_GETCOUNT, ex, userId);
            throw;
        }
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId,
        int page = 1, int pageSize = 20)
    {
        try
        {
            var all = await _repo.FindAsync(n => n.UserId == userId);
            return all
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Title = n.Title,
                    Message = n.Message,
                    ActionUrl = n.ActionUrl,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToList();
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_GETLIST, ex, userId);
            throw;
        }
    }

    public async Task MarkAsReadAsync(int notificationId, Guid userId)
    {
        try
        {
            var matches = await _repo.FindAsync(n => n.Id == notificationId && n.UserId == userId);
            var n = matches.FirstOrDefault();
            if (n == null) return;
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            _repo.Update(n);
            await _repo.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_READ, ex, new { notificationId, userId });
            throw;
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        try
        {
            var unread = await _repo.FindAsync(n => n.UserId == userId && !n.IsRead);
            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
                _repo.Update(n);
            }
            await _repo.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_READALL, ex, userId);
            throw;
        }
    }

    public async Task SendSmsNotificationAsync(Guid userId, string message)
    {
        try
        {
            // KullanÄ±cÄ±nÄ±n telefon numarasÄ±nÄ± al (User entity'sinde PhoneNumber alanÄ± olmalÄ±)
            // Åimdilik basit bir implementasyon yapalÄ±m
            await _smsService.SendAsync("905551234567", message); // Test numarasÄ±
            // Log baÅŸarÄ±lÄ±
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync("NM-006", ex, message, userId);
        }
    }

    public async Task SendFcmNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null)
    {
        try
        {
            await _fcmService.SendNotificationAsync(fcmToken, title, body, data);
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync("NM-007", ex, title);
        }
    }
}
