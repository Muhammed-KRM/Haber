namespace KursuTV.Data.Entities;

/// <summary>
/// Bir habere ait görsel veya video dosyalarını temsil eder.
/// Dosyalar ImageSharp ile işlenip MinIO/S3'e yüklenir; bu tablo sadece meta veriyi saklar.
/// </summary>
public class Media
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>S3/MinIO'daki orijinal boyut dosya URL'si.</summary>
    public string OriginalUrl { get; set; } = string.Empty;

    /// <summary>ImageSharp tarafından oluşturulan WebP manşet boyutu URL'si (1200x630).</summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>ImageSharp tarafından oluşturulan WebP liste boyutu URL'si (400x300).</summary>
    public string? ListUrl { get; set; }

    /// <summary>Erişilebilirlik ve SEO için alt text.</summary>
    public string? AltText { get; set; }

    /// <summary>Dosyanın MIME türü (image/webp, image/jpeg, video/mp4 gibi). Magic Bytes ile doğrulanır.</summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>Orijinal dosyanın byte cinsinden boyutu.</summary>
    public long FileSizeBytes { get; set; }

    /// <summary>Bu medya hangi habere aittir. Null ise medya kütüphanesindedir (bağımsız).</summary>
    public Guid? NewsId { get; set; }

    /// <summary>Bu dosyayı yükleyen kullanıcı.</summary>
    public Guid UploadedByUserId { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // --- Navigation Properties ---
    public News? News { get; set; }
    public User UploadedByUser { get; set; } = null!;
}
