using KursuTV.Data.Enums;

namespace KursuTV.Data.Entities;

/// <summary>
/// Sistemdeki tüm kullanıcıları (admin, editör, muhabir, köşe yazarı) temsil eder.
/// ASP.NET Core Identity yerine hafif, özel JWT tabanlı kimlik yönetimi kullanılmaktadır.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Giriş için kullanılan benzersiz e-posta adresi.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>BCrypt ile hashlenerek saklanır, asla plain-text olmaz.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    /// <summary>Bio / Hakkında metni. Köşe yazarları için profil sayfasında gösterilir.</summary>
    public string? Bio { get; set; }

    public string? ProfileImageUrl { get; set; }

    public UserRole Role { get; set; } = UserRole.Reporter;

    public bool IsActive { get; set; } = true;

    public bool IsEmailVerified { get; set; } = false;

    // --- Bildirim Ayarları ---
    public bool EmailNotifications { get; set; } = true;

    // --- JWT Refresh Token ---
    /// <summary>Güvenli rastgele üretilmiş refresh token değeri.</summary>
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // --- Moderasyon / Ban ---
    public int ViolationCount { get; set; } = 0;
    public DateTime? BannedUntil { get; set; }
    public string? BanReason { get; set; }

    // --- Zaman Damgaları ---
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // --- Navigation Properties ---
    /// <summary>Bu kullanıcı tarafından yazılan/yüklenen haberler.</summary>
    public ICollection<News> News { get; set; } = [];

    /// <summary>Bu kullanıcı tarafından yazılan yorumlar.</summary>
    public ICollection<Comment> Comments { get; set; } = [];
}
