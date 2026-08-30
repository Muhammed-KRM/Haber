using KursuTV.Data.Entities;

namespace KursuTV.Data.Repositories;

public interface IMediaRepository : IRepository<Media>
{
    Task<(List<Media> Items, int TotalCount)> GetPagedMediaAsync(int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);
    Task<List<Media>> GetByNewsIdAsync(Guid newsId, CancellationToken cancellationToken = default);
}
