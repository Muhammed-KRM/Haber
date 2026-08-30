namespace KursuTV.Data.Entities;

/// <summary>
/// Haberlerde kullanılan etiketleri temsil eder.
/// Elasticsearch'te fuzzy search ve ilgili haber önerileri için kullanılır.
/// </summary>
public class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>URL'de kullanılacak SEO dostu kısa ad (örn: "deprem-2026").</summary>
    public string Slug { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // --- Navigation Properties ---
    public ICollection<NewsTag> NewsTags { get; set; } = [];
}
