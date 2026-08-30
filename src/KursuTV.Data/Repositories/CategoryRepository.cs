using KursuTV.Data.Context;
using KursuTV.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KursuTV.Data.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Category>> GetActiveCategoriesWithChildrenAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(c => c.IsActive && c.ParentId == null)
            .OrderBy(c => c.SortOrder)
            .Include(c => c.Children.Where(ch => ch.IsActive).OrderBy(ch => ch.SortOrder))
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(c => c.Children.Where(ch => ch.IsActive))
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().Where(c => c.Slug == slug);
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }
}
