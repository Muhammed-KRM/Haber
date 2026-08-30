using KursuTV.Business.DTOs;

namespace KursuTV.Business.Interfaces;

public interface INewsService
{
    // Okuyucu (Public) Metodları
    Task<List<HeadlineNewsDto>> GetHeadlinesAsync(int count = 7, CancellationToken cancellationToken = default);
    Task<List<BreakingNewsDto>> GetBreakingNewsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<PagedResultDto<NewsListDto>> GetPublishedNewsPagedAsync(int pageNumber, int pageSize, int? categoryId = null, CancellationToken cancellationToken = default);
    Task<NewsDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<NewsListDto>> GetRelatedNewsAsync(Guid currentNewsId, List<int> tagIds, int count = 4, CancellationToken cancellationToken = default);
    Task<List<NewsListDto>> GetMostViewedNewsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task RecordViewAsync(Guid newsId, CancellationToken cancellationToken = default);

    // Yönetim (Admin / Editör) Metodları
    Task<Guid> CreateNewsAsync(NewsCreateDto dto, Guid authorId, CancellationToken cancellationToken = default);
    Task UpdateNewsAsync(NewsUpdateDto dto, Guid currentUserId, CancellationToken cancellationToken = default);
    Task DeleteNewsAsync(Guid newsId, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<NewsDetailDto?> GetByIdForEditAsync(Guid newsId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<NewsListDto>> GetAdminNewsPagedAsync(NewsFilterDto filter, CancellationToken cancellationToken = default);
    Task AutoSaveDraftAsync(NewsUpdateDto dto, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<int> PublishScheduledNewsAsync(CancellationToken cancellationToken = default);
    Task InvalidateSitemapCacheAsync(CancellationToken cancellationToken = default);
}
