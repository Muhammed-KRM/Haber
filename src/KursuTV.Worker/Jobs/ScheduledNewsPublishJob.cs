using KursuTV.Business.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KursuTV.Worker.Jobs;

/// <summary>
/// Her 5 dakikada bir "zamanlanmış yayın" durumundaki haberleri kontrol ederek
/// yayın tarihi gelenleri Published olarak günceller.
/// </summary>
public class ScheduledNewsPublishJob : BackgroundService
{
    private readonly ILogger<ScheduledNewsPublishJob> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    public ScheduledNewsPublishJob(
        ILogger<ScheduledNewsPublishJob> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[ScheduledNewsPublishJob] Başlatıldı. Kontrol aralığı: {Interval} dakika.", Interval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var newsService = scope.ServiceProvider.GetRequiredService<INewsService>();
                var count = await newsService.PublishScheduledNewsAsync(stoppingToken);

                if (count > 0)
                    _logger.LogInformation("[ScheduledNewsPublishJob] {Count} zamanlanmış haber yayına alındı.", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ScheduledNewsPublishJob] Zamanlanmış haber yayını sırasında hata oluştu.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
