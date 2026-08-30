using KursuTV.Business.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace KursuTV.Worker.Jobs;

/// <summary>
/// Haber okunma sayılarını belleğe (in-memory queue) alır, periyodik olarak
/// toplu güncelleme ile veritabanına yazar. Sık okunan haberlerde N+1 DB çağrısını önler.
/// </summary>
public class ViewCountBatchJob : BackgroundService
{
    private static readonly ConcurrentDictionary<Guid, int> _pendingViews = new();
    private readonly ILogger<ViewCountBatchJob> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly TimeSpan FlushInterval = TimeSpan.FromMinutes(2);

    public ViewCountBatchJob(
        ILogger<ViewCountBatchJob> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    /// <summary>Dış servislerden (API) okuma kaydı ekler.</summary>
    public static void Enqueue(Guid newsId, int count = 1)
    {
        _pendingViews.AddOrUpdate(newsId, count, (_, existing) => existing + count);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[ViewCountBatchJob] Başlatıldı. Flush aralığı: {Interval} dakika.", FlushInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(FlushInterval, stoppingToken);

            if (_pendingViews.IsEmpty) continue;

            // Snapshot al → temizle → yaz
            var snapshot = _pendingViews.ToArray();
            foreach (var key in snapshot.Select(s => s.Key))
                _pendingViews.TryRemove(key, out _);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var newsService = scope.ServiceProvider.GetRequiredService<INewsService>();

                foreach (var (newsId, _) in snapshot)
                    await newsService.RecordViewAsync(newsId, stoppingToken);

                _logger.LogInformation("[ViewCountBatchJob] {Count} haber için görüntülenme sayısı toplu güncellendi.", snapshot.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ViewCountBatchJob] Toplu görüntülenme güncelleme hatası.");
            }
        }
    }
}
