namespace KursuTV.Data.Entities;

/// <summary>
/// Haber kategorilerini temsil eder (Gündem, Spor, Ekonomi vb.).
/// İç içe kategori desteği için ParentId kullanılır.
/// </summary>
public class Category
{
    public int Id { get; set; }

    /// <summary>Kategori adı (örn: "Spor", "Ekonomi").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL'de kullanılacak SEO dostu kısa ad (örn: "spor", "ekonomi").</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Kategori açıklaması. SEO için meta description olarak kullanılabilir.</summary>
    public string? Description { get; set; }

    /// <summary>Kategori sıralama değeri. Küçük değer = önce gösterilir.</summary>
    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    /// <summary>Üst kategori ID'si. Null ise kök kategoridir.</summary>
    public int? ParentId { get; set; }

    // --- Zaman Damgaları ---
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // --- Navigation Properties ---
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = [];
    public ICollection<NewsCategory> NewsCategories { get; set; } = [];
}
