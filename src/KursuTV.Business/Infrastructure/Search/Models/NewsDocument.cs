namespace KursuTV.Business.Infrastructure.Search.Models;

public class NewsDocument
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Spot { get; set; }
    public string Content { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
    public string AuthorName { get; set; } = null!;
    public List<string> CategoryNames { get; set; } = [];
    public List<string> CategorySlugs { get; set; } = [];
    public List<string> TagNames { get; set; } = [];
    public long ViewCount { get; set; }
    public DateTime? PublishedAt { get; set; }
}
