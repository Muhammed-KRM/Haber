using Microsoft.AspNetCore.Mvc;
using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;

namespace KursuTV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<TagDto>>> Search([FromQuery] string q, [FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q)) return Ok(new List<TagDto>());
        var result = await _tagService.SearchTagsAsync(q, count, cancellationToken);
        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<TagDto>> GetBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var result = await _tagService.GetBySlugAsync(slug, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
