using KursuTV.Data.Enums;

namespace KursuTV.Data.Entities;

/// <summary>
/// Ziyaretçiler tarafından bir habere yapılan yorumları temsil eder.
/// Moderasyon akışı: Pending → Approved/Rejected/Spam.
/// </summary>
public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Content { get; set; } = string.Empty;

    /// <summary>Yorumun yapıldığı haber.</summary>
    public Guid NewsId { get; set; }

    /// <summary>Yorumu yapan kullanıcı. Null ise anonim yorum (ileride açılabilir).</summary>
    public Guid? UserId { get; set; }

    /// <summary>Kayıtlı kullanıcı değilse gösterilecek isim.</summary>
    public string? GuestName { get; set; }

    public CommentStatus Status { get; set; } = CommentStatus.Pending;

    /// <summary>Yanıt verilen üst yorum ID'si. Null ise üst düzey yorumdur.</summary>
    public Guid? ParentCommentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // --- Navigation Properties ---
    public News News { get; set; } = null!;
    public User? User { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = [];
}
