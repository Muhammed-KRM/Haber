using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Entities;
using KursuTV.Data.Repositories;

namespace KursuTV.Business.Services;

public class NotificationManager : INotificationService
{
    private const string EC_CREATE   = "NM-001";
    private const string EC_GETCOUNT = "NM-002";
    private const string EC_GETLIST  = "NM-003";
    private const string EC_READ     = "NM-004";
    private const string EC_READALL  = "NM-005";

    private readonly IRepository<Notification> _repo;
    private readonly ILogService _logService;
    private readonly KursuTV.Business.Infrastructure.Messaging.IFcmService _fcmService;

    public NotificationManager(
        IRepository<Notification> repo,
        ILogService logService,
        KursuTV.Business.Infrastructure.Messaging.IFcmService fcmService)
    {
        _repo = repo;
        _logService = logService;
        _fcmService = fcmService;
    }

    public async Task<Notification> CreateAsync(Guid userId, string type, string title,
        string message, string? actionUrl = null, string? idempotencyKey = null)
    {
        try
        {
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
            await _logService.LogFunctionErrorAsync(EC_CREATE, ex, new { userId, type, title }, userId);
            throw;
        }
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        try
        {
            var list = await _repo.FindAsync(n => n.UserId == userId && !n.IsRead);
            return list.Count();
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_GETCOUNT, ex, userId, userId);
            throw;
        }
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var list = await _repo.FindAsync(n => n.UserId == userId);
            return list
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
            await _logService.LogFunctionErrorAsync(EC_GETLIST, ex, new { userId, page, pageSize }, userId);
            throw;
        }
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, Guid userId)
    {
        try
        {
            var notif = await _repo.GetByIdAsync(notificationId);
            if (notif == null || notif.UserId != userId) return false;
            notif.IsRead = true;
            notif.ReadAt = DateTime.UtcNow;
            _repo.Update(notif);
            await _repo.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_READ, ex, new { notificationId, userId }, userId);
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
            await _logService.LogFunctionErrorAsync(EC_READALL, ex, userId, userId);
            throw;
        }
    }

    public async Task SendPushNotificationAsync(string fcmToken, string title, string body, string? clickAction = null)
    {
        await _fcmService.SendNotificationAsync(fcmToken, title, body);
    }
}
