using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;

namespace KursuTV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpPost("upload")]
    [Authorize]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB
    public async Task<ActionResult<MediaDto>> Upload(
        [FromForm] IFormFile file,
        [FromForm] string? altText,
        [FromForm] Guid? newsId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        await using var stream = file.OpenReadStream();
        var result = await _mediaService.UploadSingleFileAsync(
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            altText,
            newsId,
            userId,
            cancellationToken
        );

        return Ok(result);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PagedResultDto<MediaDto>>> GetPagedMedia(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediaService.GetPagedMediaAsync(page, pageSize, search, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _mediaService.DeleteMediaAsync(id, cancellationToken);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }
}
