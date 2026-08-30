namespace KursuTV.Data.Entities;

/// <summary>
/// News ↔ Category çoka-çok pivot tablosu.
/// Bir haber birden fazla kategoriye atanabilir.
/// </summary>
public class NewsCategory
{
    public Guid NewsId { get; set; }
    public int CategoryId { get; set; }

    // --- Navigation Properties ---
    public News News { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
