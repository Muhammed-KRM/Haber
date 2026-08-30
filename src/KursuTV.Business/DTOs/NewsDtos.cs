using System.Text.Json.Serialization;
using KursuTV.Data.Enums;

namespace KursuTV.Business.DTOs;

public class NewsCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Spot { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string? CoverImageAlt { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public NewsStatus Status { get; set; } = NewsStatus.Draft;
    public NewsType Type { get; set; } = NewsType.Article;
    public bool IsBreaking { get; set; } = false;
    public int HeadlineOrder { get; set; } = 0;
    public DateTime? ScheduledPublishAt { get; set; }
    public List<int> CategoryIds { get; set; } = new();
    public List<string> TagNames { get; set; } = new();
}

public class NewsUpdateDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Spot { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string? CoverImageAlt { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public NewsStatus Status { get; set; } = NewsStatus.Draft;
    public NewsType Type { get; set; } = NewsType.Article;
    public bool IsBreaking { get; set; } = false;
    public int HeadlineOrder { get; set; } = 0;
    public DateTime? ScheduledPublishAt { get; set; }
    public List<int> CategoryIds { get; set; } = new();
    public List<string> TagNames { get; set; } = new();
}

public record NewsDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string? Spot,
    string Content,
    string? CoverImageUrl,
    string? CoverImageAlt,
    string? MetaTitle,
    string? MetaDescription,
    NewsStatus Status,
    NewsType Type,
    bool IsBreaking,
    int HeadlineOrder,
    long ViewCount,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    Guid AuthorId,
    string AuthorName,
    string? AuthorProfileImageUrl,
    string? AuthorBio,
    List<CategoryDto> Categories,
    List<TagDto> Tags,
    List<CommentDto> Comments
);

public record NewsListDto(
    Guid Id,
    string Title,
    string Slug,
    string? Spot,
    string? CoverImageUrl,
    string? CoverImageAlt,
    NewsStatus Status,
    NewsType Type,
    bool IsBreaking,
    int HeadlineOrder,
    long ViewCount,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    string AuthorName,
    List<CategoryDto> Categories
);

public record HeadlineNewsDto(
    Guid Id,
    string Title,
    string Slug,
    string? Spot,
    string? CoverImageUrl,
    string? CoverImageAlt,
    int HeadlineOrder,
    DateTime? PublishedAt,
    string? PrimaryCategoryName,
    string? PrimaryCategorySlug
);

public record BreakingNewsDto(
    Guid Id,
    string Title,
    string Slug,
    DateTime? PublishedAt
);

public record NewsFilterDto(
    int PageNumber = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    NewsStatus? Status = null,
    int? CategoryId = null,
    Guid? AuthorId = null
);

public record PagedResultDto<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
