namespace KursuTV.Data.Enums;

/// <summary>
/// Bir haberin yayın akışındaki durumunu tanımlar.
/// Taslak → İnceleniyor → Yayında → Arşivlendi iş akışı.
/// </summary>
public enum NewsStatus
{
    /// <summary>Henüz tamamlanmamış, editör tarafından yazılmakta olan haber.</summary>
    Draft = 0,

    /// <summary>Editör tarafından onay bekleyen haber.</summary>
    PendingReview = 1,

    /// <summary>Yayınlanmış ve herkese açık haber.</summary>
    Published = 2,

    /// <summary>Yayından kaldırılmış arşivlenmiş haber.</summary>
    Archived = 3
}
