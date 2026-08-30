using KursuTV.Business.DTOs;

namespace KursuTV.Business.Interfaces;

public record NewsSearchFilterDto(
    string Query,
    string? CategorySlug = null,
    string? TagSlug = null,
    int Page = 1,
    int PageSize = 20
);

public interface ISearchService
{
    Task<PagedResultDto<NewsListDto>> SearchNewsAsync(NewsSearchFilterDto filters, CancellationToken cancellationToken = default);
    Task IndexNewsAsync(NewsDetailDto news, CancellationToken cancellationToken = default);
    Task DeleteNewsIndexAsync(Guid newsId, CancellationToken cancellationToken = default);
}
