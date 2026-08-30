namespace KursuTV.Data.Entities;

/// <summary>
/// News ↔ Tag çoka-çok pivot tablosu.
/// Elasticsearch tag indekslemesi ve ilgili haberler önerisi için kullanılır.
/// </summary>
public class NewsTag
{
    public Guid NewsId { get; set; }
    public int TagId { get; set; }

    // --- Navigation Properties ---
    public News News { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
