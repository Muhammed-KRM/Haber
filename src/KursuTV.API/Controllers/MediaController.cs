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
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<MediaDto>> Upload(
        [FromForm] UploadMediaRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        await using var stream = request.File.OpenReadStream();
        var result = await _mediaService.UploadSingleFileAsync(
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length,
            request.AltText,
            request.NewsId,
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

public class UploadMediaRequest
{
    public IFormFile File { get; set; } = null!;
    public string? AltText { get; set; }
    public Guid? NewsId { get; set; }
}
