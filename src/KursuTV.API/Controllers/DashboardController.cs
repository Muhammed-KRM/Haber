using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;

namespace KursuTV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats(CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetStatsAsync(cancellationToken);
        return Ok(result);
    }
}
