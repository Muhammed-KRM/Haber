using KursuTV.Business.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KursuTV.Worker.Jobs;

/// <summary>
/// Her gece 02:00'de sitemap.xml ve Google News sitemap dosyalarını
/// önbelleği temizleyerek yeniden oluşturulmasını sağlar.
/// </summary>
public class SitemapGeneratorJob : BackgroundService
{
    private readonly ILogger<SitemapGeneratorJob> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public SitemapGeneratorJob(
        ILogger<SitemapGeneratorJob> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[SitemapGeneratorJob] Başlatıldı. Her gece 02:00'de çalışacak.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = DateTime.Today.AddDays(1).AddHours(2);
            var delay = nextRun - now;

            if (delay.TotalMilliseconds <= 0)
                delay = TimeSpan.FromMinutes(1);

            _logger.LogInformation("[SitemapGeneratorJob] Bir sonraki çalışma: {NextRun}", nextRun);
            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var newsService = scope.ServiceProvider.GetRequiredService<INewsService>();
                // Sitemap cache'i Redis'ten temizle → bir sonraki istek yeniden oluşturur
                await newsService.InvalidateSitemapCacheAsync(stoppingToken);
                _logger.LogInformation("[SitemapGeneratorJob] Sitemap cache başarıyla temizlendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SitemapGeneratorJob] Sitemap yenileme hatası.");
            }
        }
    }
}
