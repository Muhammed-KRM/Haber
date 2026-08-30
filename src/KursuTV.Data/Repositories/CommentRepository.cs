using KursuTV.Data.Context;
using KursuTV.Data.Entities;
using KursuTV.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace KursuTV.Data.Repositories;

public class CommentRepository : GenericRepository<Comment>, ICommentRepository
{
    public CommentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Comment>> GetApprovedCommentsByNewsIdAsync(Guid newsId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(c => c.NewsId == newsId && c.Status == CommentStatus.Approved && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .Include(c => c.User)
            .Include(c => c.Replies.Where(r => r.Status == CommentStatus.Approved).OrderBy(r => r.CreatedAt))
                .ThenInclude(r => r.User)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Comment> Items, int TotalCount)> GetPendingCommentsPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().Where(c => c.Status == CommentStatus.Pending);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.News)
            .Include(c => c.User)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task UpdateCommentStatusAsync(Guid commentId, CommentStatus status, CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(c => c.Id == commentId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Status, status), cancellationToken);
    }
}
