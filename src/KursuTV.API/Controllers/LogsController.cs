using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;

namespace KursuTV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class LogsController : ControllerBase
{
    private readonly ILogService _logService;

    public LogsController(ILogService logService)
    {
        _logService = logService;
    }

    [HttpGet("endpoints")]
    public async Task<IActionResult> GetEndpointLogs([FromQuery] LogFilterRequest request, CancellationToken cancellationToken)
    {
        var result = await _logService.GetEndpointLogsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("functions")]
    public async Task<IActionResult> GetFunctionLogs([FromQuery] LogFilterRequest request, CancellationToken cancellationToken)
    {
        var result = await _logService.GetFunctionLogsAsync(request, cancellationToken);
        return Ok(result);
    }
}
