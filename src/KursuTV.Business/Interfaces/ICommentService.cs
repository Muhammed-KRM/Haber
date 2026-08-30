using KursuTV.Business.DTOs;
using KursuTV.Data.Enums;

namespace KursuTV.Business.Interfaces;

public interface ICommentService
{
    Task<List<CommentDto>> GetApprovedCommentsByNewsIdAsync(Guid newsId, CancellationToken cancellationToken = default);
    Task<Guid> AddCommentAsync(CreateCommentDto dto, Guid? currentUserId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<CommentDto>> GetPendingCommentsPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task ModerateCommentAsync(Guid commentId, CommentStatus status, CancellationToken cancellationToken = default);
    Task DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken = default);
}
