using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using KursuTV.Data.Context;
using KursuTV.Data.Enums;
using Microsoft.EntityFrameworkCore;
using KursuTV.Business.Interfaces;

namespace KursuTV.Worker.Services;

public class VitrinExpirationWorker : BackgroundService
{
    private readonly ILogger<VitrinExpirationWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public VitrinExpirationWorker(ILogger<VitrinExpirationWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("VitrinExpirationWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

                var expiredListings = await dbContext.Listings
                    .Where(l => l.IsVitrin && l.VitrinExpiresAt <= DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                if (expiredListings.Any())
                {
                    _logger.LogInformation("{Count} vitrin properties expired, updating...", expiredListings.Count);

                    foreach (var listing in expiredListings)
                    {
                        listing.IsVitrin = false;
                        listing.VitrinExpiresAt = null;
                        
                        // Ä°steÄŸe baÄŸlÄ± olarak kullanÄ±cÄ±ya "SÃ¼reniz Bitti" maili atmak iÃ§in Event fÄ±rlatÄ±labilir.
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                    
                    // Vitrin ilan listesinin Ã¶nbelleÄŸini temizle (Yeniden Ã§ekilsin)
                    await cacheService.RemoveByPatternAsync("vitrin:*");
                    
                    _logger.LogInformation("Successfully updated {Count} expired listings.", expiredListings.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing VitrinExpirationWorker.");
            }

            // Her 1 saatte bir kontrol et
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
