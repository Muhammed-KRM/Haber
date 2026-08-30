using KursuTV.Business.DTOs;

namespace KursuTV.Business.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}
