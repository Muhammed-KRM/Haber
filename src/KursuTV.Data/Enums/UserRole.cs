namespace KursuTV.Data.Enums;

/// <summary>
/// Kullanıcının sistem içindeki rolünü tanımlar.
/// RBAC (Rol Bazlı Erişim Kontrolü) için kullanılır.
/// </summary>
public enum UserRole
{
    /// <summary>Her şeye erişebilen yönetici.</summary>
    SuperAdmin = 0,

    /// <summary>Haber yayınlama yetkisine sahip yayın yönetmeni.</summary>
    Editor = 1,

    /// <summary>Sadece kendi haberlerini gönderebilen muhabir.</summary>
    Reporter = 2,

    /// <summary>Köşe yazıları yazabilen yazar.</summary>
    ColumnWriter = 3
}
