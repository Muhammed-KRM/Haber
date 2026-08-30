using KursuTV.Data.Entities;

namespace KursuTV.Data.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<List<Category>> GetActiveCategoriesWithChildrenAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default);
}
