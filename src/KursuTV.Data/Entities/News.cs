using KursuTV.Data.Enums;

namespace KursuTV.Data.Entities;

/// <summary>
/// Sistemin ana varlığı olan haberi temsil eder.
/// SEO, içerik yönetimi, manşet sıralaması ve zamanlı yayın özelliklerini destekler.
/// </summary>
public class News
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Haber başlığı. OpenGraph ve sayfa title olarak kullanılır.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// SEO dostu URL parçası. Örn: "turkiyede-secim-sonuclari-2026".
    /// SlugHelper ile otomatik üretilir ancak editör tarafından değiştirilebilir.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Haber özeti. Liste ve anasayfada gösterilir, aynı zamanda meta description olarak kullanılır.</summary>
    public string? Spot { get; set; }

    /// <summary>Haberin tam metni. TipTap editöründen gelen HTML içeriği. HtmlSanitizer ile temizlenir.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Ana görsel URL'si. ImageSharp tarafından WebP'ye çevrilmiş hali kullanılır.</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>Görsele ait erişilebilirlik metni (alt text). SEO ve erişilebilirlik için zorunlu tutulur.</summary>
    public string? CoverImageAlt { get; set; }

    // --- SEO Alanları ---
    /// <summary>Haber için özel meta title. Boşsa Title alanı kullanılır.</summary>
    public string? MetaTitle { get; set; }

    /// <summary>Haber için özel meta description. Boşsa Spot alanı kullanılır.</summary>
    public string? MetaDescription { get; set; }

    // --- Yayın Yönetimi ---
    public NewsStatus Status { get; set; } = NewsStatus.Draft;

    public NewsType Type { get; set; } = NewsType.Article;

    /// <summary>True ise son dakika bandında gösterilir (SignalR ile anlık güncellenir).</summary>
    public bool IsBreaking { get; set; } = false;

    /// <summary>Anasayfa manşet sıralaması. 0 = manşette değil. 1-5 = manşet sırası.</summary>
    public int HeadlineOrder { get; set; } = 0;

    /// <summary>İleri tarihli yayın için zamanlama. Hangfire bu alana göre otomatik yayına alır.</summary>
    public DateTime? ScheduledPublishAt { get; set; }

    /// <summary>Fiilen yayınlandığı zaman. Sitemap ve haber tarihi için kullanılır.</summary>
    public DateTime? PublishedAt { get; set; }

    // --- İstatistikler ---
    /// <summary>
    /// Okunma sayacı. Doğrudan DB'ye yazılmaz; RabbitMQ Consumer toplu olarak günceller.
    /// Bu sayede yoğun trafikte veritabanı kilitlenmez.
    /// </summary>
    public long ViewCount { get; set; } = 0;

    // --- İlişkiler ---
    /// <summary>Haberi yazan/ekleyen kullanıcının ID'si.</summary>
    public Guid AuthorId { get; set; }

    // --- Zaman Damgaları ---
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // --- Navigation Properties ---
    public User Author { get; set; } = null!;
    public ICollection<NewsCategory> NewsCategories { get; set; } = [];
    public ICollection<NewsTag> NewsTags { get; set; } = [];
    public ICollection<Media> MediaItems { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}
