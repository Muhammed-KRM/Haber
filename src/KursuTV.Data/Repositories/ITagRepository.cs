using KursuTV.Data.Entities;

namespace KursuTV.Data.Repositories;

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Tag>> GetOrCreateTagsAsync(List<string> tagNames, CancellationToken cancellationToken = default);
    Task<List<Tag>> SearchTagsAsync(string query, int count = 10, CancellationToken cancellationToken = default);
}
