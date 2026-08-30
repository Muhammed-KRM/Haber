using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using KursuTV.Data.Context;

namespace KursuTV.Worker.Services;

/// <summary>
/// 90 gÃ¼nden eski ve okunmuÅŸ bildirimleri temizler.
/// Her gece 03:00'da Ã§alÄ±ÅŸÄ±r.
/// </summary>
public class NotificationCleanupWorker : BackgroundService
{
    private readonly ILogger<NotificationCleanupWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public NotificationCleanupWorker(
        ILogger<NotificationCleanupWorker> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationCleanupWorker baÅŸlatÄ±ldÄ±.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Bir sonraki gece 03:00'a kadar bekle
            var now = DateTime.Now;
            var nextRun = now.Date.AddDays(1).AddHours(3);
            var delay = nextRun - now;
            if (delay < TimeSpan.Zero) delay = TimeSpan.FromHours(24);

            _logger.LogInformation("Bildirim temizleme bir sonraki Ã§alÄ±ÅŸma: {NextRun}", nextRun);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var cutoff = DateTime.UtcNow;

                // ExpiresAt geÃ§miÅŸ bildirimleri sil (90 gÃ¼n)
                var expired = await db.Notifications
                    .Where(n => n.ExpiresAt < cutoff)
                    .ToListAsync(stoppingToken);

                if (expired.Any())
                {
                    db.Notifications.RemoveRange(expired);
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("{Count} eski bildirim silindi.", expired.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NotificationCleanupWorker hatasÄ±.");
            }
        }
    }
}
