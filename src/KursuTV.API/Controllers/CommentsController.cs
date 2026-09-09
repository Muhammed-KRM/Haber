using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Enums;

namespace KursuTV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("news/{newsId:guid}")]
    public async Task<ActionResult<List<CommentDto>>> GetApprovedComments(Guid newsId, CancellationToken cancellationToken = default)
    {
        var result = await _commentService.GetApprovedCommentsByNewsIdAsync(newsId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> AddComment([FromBody] CreateCommentDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var id = await _commentService.AddCommentAsync(dto, userId, cancellationToken);
        return Ok(new { commentId = id, message = "Yorumunuz moderatör onayına gönderildi." });
    }

    [HttpGet("admin/pending")]
    [Authorize]
    public async Task<ActionResult<PagedResultDto<CommentDto>>> GetPendingComments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _commentService.GetPendingCommentsPagedAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPost("admin/moderate")]
    [Authorize]
    public async Task<IActionResult> ModerateComment([FromBody] CommentModerateDto dto, CancellationToken cancellationToken = default)
    {
        await _commentService.ModerateCommentAsync(dto.CommentId, dto.Status, cancellationToken);
        return NoContent();
    }

    [HttpDelete("admin/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(Guid id, CancellationToken cancellationToken = default)
    {
        await _commentService.DeleteCommentAsync(id, cancellationToken);
        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : null;
    }
}
