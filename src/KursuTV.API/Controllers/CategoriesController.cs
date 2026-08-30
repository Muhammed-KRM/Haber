using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;

namespace KursuTV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("tree")]
    public async Task<ActionResult<List<CategoryDto>>> GetCategoryTree(CancellationToken cancellationToken = default)
    {
        var result = await _categoryService.GetAllActiveTreeAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("flat")]
    public async Task<ActionResult<List<CategoryDto>>> GetFlatList(CancellationToken cancellationToken = default)
    {
        var result = await _categoryService.GetAllFlatAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _categoryService.GetByIdAsync(id, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<CategoryDto>> GetBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var result = await _categoryService.GetBySlugAsync(slug, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<int>> Create([FromBody] CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var id = await _categoryService.CreateCategoryAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        await _categoryService.UpdateCategoryAsync(dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await _categoryService.DeleteCategoryAsync(id, cancellationToken);
        return NoContent();
    }
}
