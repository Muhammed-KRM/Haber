namespace KursuTV.Data.Enums;

/// <summary>
/// Yorum sistemindeki bir yorumun moderasyon durumunu tanımlar.
/// </summary>
public enum CommentStatus
{
    /// <summary>Moderatör onayı bekleniyor.</summary>
    Pending = 0,

    /// <summary>Onaylandı ve herkese görünür.</summary>
    Approved = 1,

    /// <summary>Reddedildi ve gizlendi.</summary>
    Rejected = 2,

    /// <summary>Spam olarak işaretlendi.</summary>
    Spam = 3
}
