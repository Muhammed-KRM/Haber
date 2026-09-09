using KursuTV.Data.Context;
using KursuTV.Data.Entities;
using KursuTV.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace KursuTV.Data.Repositories;

/// <summary>
/// PostgreSQL ve EF Core üzerinde optimize edilmiş haber veri erişim sınıfı.
/// Performans için AsNoTracking ve ExecuteUpdateAsync kullanılır.
/// </summary>
public class NewsRepository : GenericRepository<News>, INewsRepository
{
    public NewsRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<News>> GetHeadlinesAsync(int count = 7, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(n => n.Status == NewsStatus.Published && n.HeadlineOrder > 0)
            .OrderBy(n => n.HeadlineOrder)
            .ThenByDescending(n => n.PublishedAt)
            .Take(count)
            .Include(n => n.Author)
            .Include(n => n.NewsCategories)
                .ThenInclude(nc => nc.Category)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<News>> GetBreakingNewsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(n => n.Status == NewsStatus.Published && n.IsBreaking)
            .OrderByDescending(n => n.PublishedAt)
            .Take(count)
            .Select(n => new News
            {
                Id = n.Id,
                Title = n.Title,
                Slug = n.Slug,
                PublishedAt = n.PublishedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<News> Items, int TotalCount)> GetPublishedPagedAsync(
        int pageNumber,
        int pageSize,
        int? categoryId = null,
        NewsType? type = null,
        Guid? authorId = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking()
            .Where(n => n.Status == NewsStatus.Published);

        if (categoryId.HasValue)
        {
            query = query.Where(n => n.NewsCategories.Any(nc => nc.CategoryId == categoryId.Value));
        }

        if (type.HasValue)
        {
            query = query.Where(n => n.Type == type.Value);
        }

        if (authorId.HasValue)
        {
            query = query.Where(n => n.AuthorId == authorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(n => n.Title.ToLower().Contains(term) || (n.Spot != null && n.Spot.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.PublishedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(n => n.Author)
            .Include(n => n.NewsCategories)
                .ThenInclude(nc => nc.Category)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<News?> GetBySlugAsync(string slug, bool includeDrafts = false, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();

        if (!includeDrafts)
        {
            query = query.Where(n => n.Status == NewsStatus.Published);
        }

        return await query
            .Where(n => n.Slug == slug)
            .Include(n => n.Author)
            .Include(n => n.NewsCategories)
                .ThenInclude(nc => nc.Category)
            .Include(n => n.NewsTags)
                .ThenInclude(nt => nt.Tag)
            .Include(n => n.MediaItems)
            .Include(n => n.Comments.Where(c => c.Status == CommentStatus.Approved))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<News>> GetRelatedNewsAsync(Guid currentNewsId, List<int> tagIds, int count = 4, CancellationToken cancellationToken = default)
    {
        if (tagIds == null || tagIds.Count == 0)
        {
            // Etiket yoksa son yayınlanan diğer haberleri getir
            return await _dbSet.AsNoTracking()
                .Where(n => n.Status == NewsStatus.Published && n.Id != currentNewsId)
                .OrderByDescending(n => n.PublishedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        return await _dbSet.AsNoTracking()
            .Where(n => n.Status == NewsStatus.Published && n.Id != currentNewsId)
            .Where(n => n.NewsTags.Any(nt => tagIds.Contains(nt.TagId)))
            .OrderByDescending(n => n.PublishedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<News>> GetMostViewedNewsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(n => n.Status == NewsStatus.Published)
            .OrderByDescending(n => n.ViewCount)
            .ThenByDescending(n => n.PublishedAt)
            .Take(count)
            .Include(n => n.NewsCategories)
                .ThenInclude(nc => nc.Category)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<News>> GetPendingScheduledNewsAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(n => n.Status == NewsStatus.Draft 
                     && n.ScheduledPublishAt.HasValue 
                     && n.ScheduledPublishAt.Value <= nowUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task IncrementViewCountAsync(Guid newsId, int incrementBy = 1, CancellationToken cancellationToken = default)
    {
        // Entity yüklemeden doğrudan SQL UPDATE (ExecuteUpdateAsync) ile atomik ve kilitlenmesiz artırım
        await _dbSet
            .Where(n => n.Id == newsId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.ViewCount, n => n.ViewCount + incrementBy), cancellationToken);
    }

    public async Task<(List<News> Items, int TotalCount)> GetAdminPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        NewsStatus? status,
        int? categoryId,
        Guid? authorId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(n => n.Title.ToLower().Contains(search) || n.Slug.Contains(search));
        }

        if (status.HasValue)
        {
            query = query.Where(n => n.Status == status.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(n => n.NewsCategories.Any(nc => nc.CategoryId == categoryId.Value));
        }

        if (authorId.HasValue)
        {
            query = query.Where(n => n.AuthorId == authorId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(n => n.Author)
            .Include(n => n.NewsCategories)
                .ThenInclude(nc => nc.Category)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> PublishScheduledNewsAsync(CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTime.UtcNow;

        return await _dbSet
            .Where(n => n.Status == NewsStatus.Scheduled && n.ScheduledPublishAt <= nowUtc)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.Status, NewsStatus.Published)
                .SetProperty(n => n.PublishedAt, nowUtc)
                .SetProperty(n => n.UpdatedAt, nowUtc),
            cancellationToken);
    }
}

