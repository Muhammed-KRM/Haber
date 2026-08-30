namespace KursuTV.Data.Enums;

/// <summary>
/// Haberin içerik türünü tanımlar.
/// Her tür farklı bir görünüm şablonuna yönlendirilebilir.
/// </summary>
public enum NewsType
{
    /// <summary>Standart metin tabanlı haber.</summary>
    Article = 0,

    /// <summary>Birden fazla fotoğraftan oluşan galeri haberi.</summary>
    Gallery = 1,

    /// <summary>Video içerikli haber.</summary>
    Video = 2,

    /// <summary>Son dakika duyurusu.</summary>
    Breaking = 3,

    /// <summary>Köşe yazısı / Yorum yazısı.</summary>
    Column = 4
}
