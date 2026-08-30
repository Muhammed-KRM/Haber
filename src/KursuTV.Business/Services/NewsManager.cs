using KursuTV.Business.DTOs;
using KursuTV.Business.Exceptions;
using KursuTV.Business.Helpers;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Entities;
using KursuTV.Data.Enums;
using KursuTV.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace KursuTV.Business.Services;

public class NewsManager : INewsService
{
    private readonly INewsRepository _newsRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<NewsManager> _logger;

    private const string HeadlinesCacheKey = "news:headlines";
    private const string BreakingNewsCacheKey = "news:breaking";
    private const string CachePatternPrefix = "news:*";

    public NewsManager(
        INewsRepository newsRepository,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository,
        ICacheService cacheService,
        ILogger<NewsManager> logger)
    {
        _newsRepository = newsRepository;
        _categoryRepository = categoryRepository;
        _tagRepository = tagRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<List<HeadlineNewsDto>> GetHeadlinesAsync(int count = 7, CancellationToken cancellationToken = default)
    {
        var cached = await _cacheService.GetAsync<List<HeadlineNewsDto>>(HeadlinesCacheKey);
        if (cached != null)
        {
            return cached;
        }

        var newsList = await _newsRepository.GetHeadlinesAsync(count, cancellationToken);
        var result = newsList.Select(n => new HeadlineNewsDto(
            n.Id,
            n.Title,
            n.Slug,
            n.Spot,
            n.CoverImageUrl,
            n.CoverImageAlt,
            n.HeadlineOrder,
            n.PublishedAt,
            n.NewsCategories.FirstOrDefault()?.Category?.Name,
            n.NewsCategories.FirstOrDefault()?.Category?.Slug
        )).ToList();

        await _cacheService.SetAsync(HeadlinesCacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<List<BreakingNewsDto>> GetBreakingNewsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var cached = await _cacheService.GetAsync<List<BreakingNewsDto>>(BreakingNewsCacheKey);
        if (cached != null)
        {
            return cached;
        }

        var newsList = await _newsRepository.GetBreakingNewsAsync(count, cancellationToken);
        var result = newsList.Select(n => new BreakingNewsDto(
            n.Id,
            n.Title,
            n.Slug,
            n.PublishedAt
        )).ToList();

        await _cacheService.SetAsync(BreakingNewsCacheKey, result, TimeSpan.FromMinutes(2));
        return result;
    }

    public async Task<PagedResultDto<NewsListDto>> GetPublishedNewsPagedAsync(
        int pageNumber,
        int pageSize,
        int? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"news:paged:{pageNumber}:{pageSize}:{categoryId}";
        var cached = await _cacheService.GetAsync<PagedResultDto<NewsListDto>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var (items, totalCount) = await _newsRepository.GetPublishedPagedAsync(pageNumber, pageSize, categoryId, cancellationToken);

        var dtos = items.Select(n => new NewsListDto(
            n.Id,
            n.Title,
            n.Slug,
            n.Spot,
            n.CoverImageUrl,
            n.CoverImageAlt,
            n.Status,
            n.Type,
            n.IsBreaking,
            n.HeadlineOrder,
            n.ViewCount,
            n.PublishedAt,
            n.CreatedAt,
            n.Author?.FullName ?? "Editör",
            n.NewsCategories.Select(nc => new CategoryDto(
                nc.Category.Id,
                nc.Category.Name,
                nc.Category.Slug,
                nc.Category.Description,
                nc.Category.SortOrder,
                nc.Category.IsActive,
                nc.Category.ParentId,
                null
            )).ToList()
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var result = new PagedResultDto<NewsListDto>(dtos, totalCount, pageNumber, pageSize, totalPages);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(3));
        return result;
    }

    public async Task<NewsDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"news:slug:{slug}";
        var cached = await _cacheService.GetAsync<NewsDetailDto>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var news = await _newsRepository.GetBySlugAsync(slug, includeDrafts: false, cancellationToken);
        if (news == null)
        {
            return null;
        }

        var dto = MapToDetailDto(news);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));
        return dto;
    }

    public async Task<List<NewsListDto>> GetRelatedNewsAsync(Guid currentNewsId, List<int> tagIds, int count = 4, CancellationToken cancellationToken = default)
    {
        var items = await _newsRepository.GetRelatedNewsAsync(currentNewsId, tagIds, count, cancellationToken);
        return items.Select(n => new NewsListDto(
            n.Id,
            n.Title,
            n.Slug,
            n.Spot,
            n.CoverImageUrl,
            n.CoverImageAlt,
            n.Status,
            n.Type,
            n.IsBreaking,
            n.HeadlineOrder,
            n.ViewCount,
            n.PublishedAt,
            n.CreatedAt,
            n.Author?.FullName ?? "Editör",
            new List<CategoryDto>()
        )).ToList();
    }

    public async Task<List<NewsListDto>> GetMostViewedNewsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"news:mostviewed:{count}";
        var cached = await _cacheService.GetAsync<List<NewsListDto>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var items = await _newsRepository.GetMostViewedNewsAsync(count, cancellationToken);
        var result = items.Select(n => new NewsListDto(
            n.Id,
            n.Title,
            n.Slug,
            n.Spot,
            n.CoverImageUrl,
            n.CoverImageAlt,
            n.Status,
            n.Type,
            n.IsBreaking,
            n.HeadlineOrder,
            n.ViewCount,
            n.PublishedAt,
            n.CreatedAt,
            n.Author?.FullName ?? "Editör",
            n.NewsCategories.Select(nc => new CategoryDto(
                nc.Category.Id,
                nc.Category.Name,
                nc.Category.Slug,
                nc.Category.Description,
                nc.Category.SortOrder,
                nc.Category.IsActive,
                nc.Category.ParentId,
                null
            )).ToList()
        )).ToList();

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
        return result;
    }

    public async Task RecordViewAsync(Guid newsId, CancellationToken cancellationToken = default)
    {
        await _newsRepository.IncrementViewCountAsync(newsId, 1, cancellationToken);
    }

    public async Task<Guid> CreateNewsAsync(NewsCreateDto dto, Guid authorId, CancellationToken cancellationToken = default)
    {
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? SlugHelper.GenerateSlug(dto.Title)
            : SlugHelper.GenerateSlug(dto.Slug);

        // Benzersiz slug oluştur (varsa sonuna benzersiz ek koy)
        var existing = await _newsRepository.GetBySlugAsync(slug, includeDrafts: true, cancellationToken);
        if (existing != null)
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks % 10000}";
        }

        var news = new News
        {
            Id = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            Slug = slug,
            Spot = dto.Spot?.Trim(),
            Content = dto.Content,
            CoverImageUrl = dto.CoverImageUrl,
            CoverImageAlt = dto.CoverImageAlt ?? dto.Title,
            MetaTitle = string.IsNullOrWhiteSpace(dto.MetaTitle) ? dto.Title : dto.MetaTitle,
            MetaDescription = string.IsNullOrWhiteSpace(dto.MetaDescription) ? dto.Spot : dto.MetaDescription,
            Status = dto.Status,
            Type = dto.Type,
            IsBreaking = dto.IsBreaking,
            HeadlineOrder = dto.HeadlineOrder,
            ScheduledPublishAt = dto.ScheduledPublishAt,
            PublishedAt = dto.Status == NewsStatus.Published ? DateTime.UtcNow : null,
            AuthorId = authorId,
            CreatedAt = DateTime.UtcNow
        };

        // Kategorileri bağla
        if (dto.CategoryIds != null && dto.CategoryIds.Count > 0)
        {
            foreach (var catId in dto.CategoryIds.Distinct())
            {
                news.NewsCategories.Add(new NewsCategory { NewsId = news.Id, CategoryId = catId });
            }
        }

        // Etiketleri bağla
        if (dto.TagNames != null && dto.TagNames.Count > 0)
        {
            var tags = await _tagRepository.GetOrCreateTagsAsync(dto.TagNames, cancellationToken);
            foreach (var tag in tags)
            {
                news.NewsTags.Add(new NewsTag { NewsId = news.Id, TagId = tag.Id });
            }
        }

        await _newsRepository.AddAsync(news);
        await _newsRepository.SaveChangesAsync();

        await InvalidateCachesAsync();
        _logger.LogInformation("Haber oluşturuldu: {NewsId} - {Title} (Yazar: {AuthorId})", news.Id, news.Title, authorId);

        return news.Id;
    }

    public async Task UpdateNewsAsync(NewsUpdateDto dto, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var news = await _newsRepository.GetBySlugAsync(dto.Slug ?? string.Empty, includeDrafts: true, cancellationToken)
                   ?? await _newsRepository.GetByIdAsync(dto.Id, cancellationToken);

        if (news == null)
        {
            throw new NotFoundException("Güncellenecek haber bulunamadı.");
        }

        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? SlugHelper.GenerateSlug(dto.Title)
            : SlugHelper.GenerateSlug(dto.Slug);

        // Eski slug cache'ini temizle
        await _cacheService.RemoveAsync($"news:slug:{news.Slug}");

        news.Title = dto.Title.Trim();
        news.Slug = slug;
        news.Spot = dto.Spot?.Trim();
        news.Content = dto.Content;
        news.CoverImageUrl = dto.CoverImageUrl;
        news.CoverImageAlt = dto.CoverImageAlt ?? dto.Title;
        news.MetaTitle = string.IsNullOrWhiteSpace(dto.MetaTitle) ? dto.Title : dto.MetaTitle;
        news.MetaDescription = string.IsNullOrWhiteSpace(dto.MetaDescription) ? dto.Spot : dto.MetaDescription;
        news.Type = dto.Type;
        news.IsBreaking = dto.IsBreaking;
        news.HeadlineOrder = dto.HeadlineOrder;
        news.ScheduledPublishAt = dto.ScheduledPublishAt;
        news.UpdatedAt = DateTime.UtcNow;

        if (news.Status != NewsStatus.Published && dto.Status == NewsStatus.Published)
        {
            news.PublishedAt = DateTime.UtcNow;
        }
        news.Status = dto.Status;

        // Kategorileri güncelle
        news.NewsCategories.Clear();
        if (dto.CategoryIds != null)
        {
            foreach (var catId in dto.CategoryIds.Distinct())
            {
                news.NewsCategories.Add(new NewsCategory { NewsId = news.Id, CategoryId = catId });
            }
        }

        // Etiketleri güncelle
        news.NewsTags.Clear();
        if (dto.TagNames != null && dto.TagNames.Count > 0)
        {
            var tags = await _tagRepository.GetOrCreateTagsAsync(dto.TagNames, cancellationToken);
            foreach (var tag in tags)
            {
                news.NewsTags.Add(new NewsTag { NewsId = news.Id, TagId = tag.Id });
            }
        }

        _newsRepository.Update(news);
        await _newsRepository.SaveChangesAsync();

        await InvalidateCachesAsync();
        _logger.LogInformation("Haber güncellendi: {NewsId} - {Title}", news.Id, news.Title);
    }

    public async Task DeleteNewsAsync(Guid newsId, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var news = await _newsRepository.GetByIdAsync(newsId, cancellationToken);
        if (news == null)
        {
            throw new NotFoundException("Silinecek haber bulunamadı.");
        }

        _newsRepository.Delete(news);
        await _newsRepository.SaveChangesAsync();

        await _cacheService.RemoveAsync($"news:slug:{news.Slug}");
        await InvalidateCachesAsync();
        _logger.LogInformation("Haber silindi: {NewsId}", newsId);
    }

    public async Task<NewsDetailDto?> GetByIdForEditAsync(Guid newsId, CancellationToken cancellationToken = default)
    {
        var news = await _newsRepository.GetByIdAsync(newsId, cancellationToken);
        return news == null ? null : MapToDetailDto(news);
    }

    public async Task<PagedResultDto<NewsListDto>> GetAdminNewsPagedAsync(NewsFilterDto filter, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _newsRepository.GetAdminPagedAsync(
            filter.PageNumber,
            filter.PageSize,
            filter.SearchTerm,
            filter.Status,
            filter.CategoryId,
            filter.AuthorId,
            cancellationToken);

        var dtos = items.Select(n => new NewsListDto(
            n.Id,
            n.Title,
            n.Slug,
            n.Spot,
            n.CoverImageUrl,
            n.CoverImageAlt,
            n.Status,
            n.Type,
            n.IsBreaking,
            n.HeadlineOrder,
            n.ViewCount,
            n.PublishedAt,
            n.CreatedAt,
            n.Author?.FullName ?? "Editör",
            n.NewsCategories.Select(nc => new CategoryDto(
                nc.Category.Id,
                nc.Category.Name,
                nc.Category.Slug,
                nc.Category.Description,
                nc.Category.SortOrder,
                nc.Category.IsActive,
                nc.Category.ParentId,
                null
            )).ToList()
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);
        return new PagedResultDto<NewsListDto>(dtos, totalCount, filter.PageNumber, filter.PageSize, totalPages);
    }

    public async Task AutoSaveDraftAsync(NewsUpdateDto dto, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var news = await _newsRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (news != null)
        {
            news.Title = dto.Title.Trim();
            news.Spot = dto.Spot?.Trim();
            news.Content = dto.Content;
            news.CoverImageUrl = dto.CoverImageUrl;
            news.UpdatedAt = DateTime.UtcNow;
            _newsRepository.Update(news);
            await _newsRepository.SaveChangesAsync();
        }
    }

    // PublishScheduledNewsAsync → bkz. dosyanın sonundaki Task<int> döndüren implementasyon

    private async Task InvalidateCachesAsync()
    {
        await _cacheService.RemoveAsync(HeadlinesCacheKey);
        await _cacheService.RemoveAsync(BreakingNewsCacheKey);
        await _cacheService.RemoveByPatternAsync(CachePatternPrefix);
    }

    private static NewsDetailDto MapToDetailDto(News news)
    {
        return new NewsDetailDto(
            news.Id,
            news.Title,
            news.Slug,
            news.Spot,
            news.Content,
            news.CoverImageUrl,
            news.CoverImageAlt,
            news.MetaTitle,
            news.MetaDescription,
            news.Status,
            news.Type,
            news.IsBreaking,
            news.HeadlineOrder,
            news.ViewCount,
            news.PublishedAt,
            news.CreatedAt,
            news.AuthorId,
            news.Author?.FullName ?? "Editör",
            news.Author?.ProfileImageUrl,
            news.Author?.Bio,
            news.NewsCategories?.Select(nc => new CategoryDto(
                nc.Category.Id,
                nc.Category.Name,
                nc.Category.Slug,
                nc.Category.Description,
                nc.Category.SortOrder,
                nc.Category.IsActive,
                nc.Category.ParentId,
                null
            )).ToList() ?? new List<CategoryDto>(),
            news.NewsTags?.Select(nt => new TagDto(
                nt.Tag.Id,
                nt.Tag.Name,
                nt.Tag.Slug
            )).ToList() ?? new List<TagDto>(),
            news.Comments?.Select(c => new CommentDto(
                c.Id,
                c.Content,
                c.NewsId,
                c.UserId,
                c.User?.FullName ?? c.GuestName ?? "Ziyaretçi",
                c.User?.ProfileImageUrl,
                c.Status,
                c.CreatedAt,
                null
            )).ToList() ?? new List<CommentDto>()
        );
    }

    /// <inheritdoc/>
    public async Task<int> PublishScheduledNewsAsync(CancellationToken cancellationToken = default)
    {
        var count = await _newsRepository.PublishScheduledNewsAsync(cancellationToken);
        if (count > 0)
        {
            // Manşet ve son dakika cache'ini temizle
            await _cacheService.RemoveAsync(HeadlinesCacheKey);
            await _cacheService.RemoveAsync(BreakingNewsCacheKey);
            _logger.LogInformation("[NewsManager.PublishScheduledNewsAsync] {Count} haber yayına alındı.", count);
        }
        return count;
    }

    /// <inheritdoc/>
    public async Task InvalidateSitemapCacheAsync(CancellationToken cancellationToken = default)
    {
        await _cacheService.RemoveAsync("sitemap:xml");
        await _cacheService.RemoveAsync("sitemap:google-news");
        _logger.LogInformation("[NewsManager.InvalidateSitemapCacheAsync] Sitemap cache temizlendi.");
    }
}

