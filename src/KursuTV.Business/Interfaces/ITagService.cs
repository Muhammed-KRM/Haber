using KursuTV.Business.DTOs;

namespace KursuTV.Business.Interfaces;

public interface ITagService
{
    Task<List<TagDto>> SearchTagsAsync(string query, int count = 10, CancellationToken cancellationToken = default);
    Task<TagDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
