using KursuTV.Data.Context;
using KursuTV.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KursuTV.Data.Repositories;

public class MediaRepository : GenericRepository<Media>, IMediaRepository
{
    public MediaRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<(List<Media> Items, int TotalCount)> GetPagedMediaAsync(int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(m => (m.AltText != null && m.AltText.ToLower().Contains(search)) || m.OriginalUrl.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(m => m.UploadedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.UploadedByUser)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<Media>> GetByNewsIdAsync(Guid newsId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(m => m.NewsId == newsId)
            .OrderBy(m => m.UploadedAt)
            .ToListAsync(cancellationToken);
    }
}
