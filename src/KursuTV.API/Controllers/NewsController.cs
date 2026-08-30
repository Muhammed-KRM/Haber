using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Enums;

namespace KursuTV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly INewsService _newsService;

    public NewsController(INewsService newsService)
    {
        _newsService = newsService;
    }

    // ==========================================
    // OKUYUCU (PUBLIC) ENDPOINT'LERİ
    // ==========================================

    /// <summary>
    /// Anasayfa manşet haberlerini sıralı getirir.
    /// </summary>
    [HttpGet("headlines")]
    public async Task<ActionResult<List<HeadlineNewsDto>>> GetHeadlines([FromQuery] int count = 7, CancellationToken cancellationToken = default)
    {
        var result = await _newsService.GetHeadlinesAsync(count, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Kayan son dakika bandında gösterilecek haberleri getirir.
    /// </summary>
    [HttpGet("breaking")]
    public async Task<ActionResult<List<BreakingNewsDto>>> GetBreakingNews([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var result = await _newsService.GetBreakingNewsAsync(count, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Yayınlanmış haberleri sayfalı olarak listeler. Kategoriye göre filtrelenebilir.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<NewsListDto>>> GetPublishedNews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _newsService.GetPublishedNewsPagedAsync(page, pageSize, categoryId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// SEO dostu slug ile haber detayını ve onaylı yorumlarını getirir.
    /// </summary>
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<NewsDetailDto>> GetBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var result = await _newsService.GetBySlugAsync(slug, cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = "Haber bulunamadı." });
        }
        return Ok(result);
    }

    /// <summary>
    /// En çok okunan popüler haberleri getirir.
    /// </summary>
    [HttpGet("most-viewed")]
    public async Task<ActionResult<List<NewsListDto>>> GetMostViewed([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var result = await _newsService.GetMostViewedNewsAsync(count, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Haberin okunma sayısını arka planda artırır.
    /// </summary>
    [HttpPost("{id:guid}/view")]
    public async Task<IActionResult> RecordView(Guid id, CancellationToken cancellationToken = default)
    {
        await _newsService.RecordViewAsync(id, cancellationToken);
        return NoContent();
    }

    // ==========================================
    // YÖNETİM (ADMIN / EDITÖR / YAZAR) ENDPOINT'LERİ
    // ==========================================

    /// <summary>
    /// Admin paneli için filtrelenmiş haber listesi döner.
    /// </summary>
    [HttpGet("admin/list")]
    [Authorize]
    public async Task<ActionResult<PagedResultDto<NewsListDto>>> GetAdminNews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] NewsStatus? status = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] Guid? authorId = null,
        CancellationToken cancellationToken = default)
    {
        var filter = new NewsFilterDto(page, pageSize, search, status, categoryId, authorId);
        var result = await _newsService.GetAdminNewsPagedAsync(filter, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Düzenleme formu için haber detayını çeker.
    /// </summary>
    [HttpGet("admin/{id:guid}")]
    [Authorize]
    public async Task<ActionResult<NewsDetailDto>> GetByIdForEdit(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _newsService.GetByIdForEditAsync(id, cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = "Haber bulunamadı." });
        }
        return Ok(result);
    }

    /// <summary>
    /// Yeni haber oluşturur (Taslak veya doğrudan yayında).
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> CreateNews([FromBody] NewsCreateDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var newsId = await _newsService.CreateNewsAsync(dto, userId, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new { id = newsId });
    }

    /// <summary>
    /// Mevcut haberi günceller.
    /// </summary>
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateNews([FromBody] NewsUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        await _newsService.UpdateNewsAsync(dto, userId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Editör yazarken her 15 saniyede bir otomatik taslak kaydeder.
    /// </summary>
    [HttpPost("auto-save")]
    [Authorize]
    public async Task<IActionResult> AutoSave([FromBody] NewsUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        await _newsService.AutoSaveDraftAsync(dto, userId, cancellationToken);
        return Ok(new { success = true, savedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Haberi siler.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteNews(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        await _newsService.DeleteNewsAsync(id, userId, cancellationToken);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }
}
