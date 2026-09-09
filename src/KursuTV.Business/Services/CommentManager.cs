using KursuTV.Business.DTOs;
using KursuTV.Business.Exceptions;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Entities;
using KursuTV.Data.Enums;
using KursuTV.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace KursuTV.Business.Services;

public class CommentManager : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICacheService _cacheService;
    private readonly IAiModerationService _aiModerationService;
    private readonly ILogger<CommentManager> _logger;

    public CommentManager(
        ICommentRepository commentRepository,
        ICacheService cacheService,
        IAiModerationService aiModerationService,
        ILogger<CommentManager> logger)
    {
        _commentRepository = commentRepository;
        _cacheService = cacheService;
        _aiModerationService = aiModerationService;
        _logger = logger;
    }

    public async Task<List<CommentDto>> GetApprovedCommentsByNewsIdAsync(Guid newsId, CancellationToken cancellationToken = default)
    {
        var comments = await _commentRepository.GetApprovedCommentsByNewsIdAsync(newsId, cancellationToken);
        return comments.Select(c => new CommentDto(
            c.Id,
            c.Content,
            c.NewsId,
            c.UserId,
            c.User?.FullName ?? c.GuestName ?? "Ziyaretçi",
            c.User?.ProfileImageUrl,
            c.Status,
            c.CreatedAt,
            c.Replies.Select(r => new CommentDto(
                r.Id,
                r.Content,
                r.NewsId,
                r.UserId,
                r.User?.FullName ?? r.GuestName ?? "Ziyaretçi",
                r.User?.ProfileImageUrl,
                r.Status,
                r.CreatedAt,
                null
            )).ToList()
        )).ToList();
    }

    public async Task<Guid> AddCommentAsync(CreateCommentDto dto, Guid? currentUserId, CancellationToken cancellationToken = default)
    {
        // 1. AI Moderasyon Kontrolü
        var moderationResult = await _aiModerationService.AnalyzeContentAsync(dto.Content, cancellationToken);
        if (!moderationResult.IsSafe)
        {
            _logger.LogWarning("AI tarafından yorum reddedildi. Sebep: {Reason}", moderationResult.Reason);
            throw new BusinessException($"Yorumunuz reddedildi: {moderationResult.Reason}");
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            NewsId = dto.NewsId,
            Content = dto.Content.Trim(),
            UserId = currentUserId,
            GuestName = currentUserId == null ? (dto.GuestName?.Trim() ?? "Ziyaretçi") : null,
            Status = CommentStatus.Pending, // Moderasyon onayına düşer
            ParentCommentId = dto.ParentCommentId,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment);
        await _commentRepository.SaveChangesAsync();

        _logger.LogInformation("Yeni yorum eklendi (Onay Bekliyor): {CommentId} - Haber: {NewsId}", comment.Id, comment.NewsId);
        return comment.Id;
    }

    public async Task<PagedResultDto<CommentDto>> GetPendingCommentsPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _commentRepository.GetPendingCommentsPagedAsync(pageNumber, pageSize, cancellationToken);
        var dtos = items.Select(c => new CommentDto(
            c.Id,
            c.Content,
            c.NewsId,
            c.UserId,
            c.User?.FullName ?? c.GuestName ?? "Ziyaretçi",
            c.User?.ProfileImageUrl,
            c.Status,
            c.CreatedAt,
            null
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResultDto<CommentDto>(dtos, totalCount, pageNumber, pageSize, totalPages);
    }

    public async Task ModerateCommentAsync(Guid commentId, CommentStatus status, CancellationToken cancellationToken = default)
    {
        await _commentRepository.UpdateCommentStatusAsync(commentId, status, cancellationToken);
        _logger.LogInformation("Yorum durumu güncellendi: {CommentId} -> {Status}", commentId, status);
    }

    public async Task DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken);
        if (comment == null)
        {
            throw new NotFoundException("Silinecek yorum bulunamadı.");
        }

        _commentRepository.Delete(comment);
        await _commentRepository.SaveChangesAsync();
        _logger.LogInformation("Yorum silindi: {CommentId}", commentId);
    }
}
