using KursuTV.Data.Entities;
using KursuTV.Data.Enums;

namespace KursuTV.Data.Repositories;

public interface ICommentRepository : IRepository<Comment>
{
    Task<List<Comment>> GetApprovedCommentsByNewsIdAsync(Guid newsId, CancellationToken cancellationToken = default);
    Task<(List<Comment> Items, int TotalCount)> GetPendingCommentsPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task UpdateCommentStatusAsync(Guid commentId, CommentStatus status, CancellationToken cancellationToken = default);
}
