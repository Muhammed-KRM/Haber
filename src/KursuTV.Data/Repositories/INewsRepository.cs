using KursuTV.Data.Entities;
using KursuTV.Data.Enums;

namespace KursuTV.Data.Repositories;

/// <summary>
/// Haber veri erişim operasyonlarını yöneten repository arayüzü.
/// Performans optimizasyonu için AsNoTracking ve özel projeksiyon metodları barındırır.
/// </summary>
public interface INewsRepository : IRepository<News>
{
    /// <summary>
    /// Anasayfa manşetinde yer alan aktif haberleri sıralı olarak getirir.
    /// </summary>
    Task<List<News>> GetHeadlinesAsync(int count = 7, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kayan son dakika bandında gösterilecek son dakika haberlerini getirir.
    /// </summary>
    Task<List<News>> GetBreakingNewsAsync(int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kategoriye göre veya genel son yayınlanan haberleri sayfalı olarak getirir.
    /// </summary>
    Task<(List<News> Items, int TotalCount)> GetPublishedPagedAsync(
        int pageNumber,
        int pageSize,
        int? categoryId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// SEO dostu URL slug ile haberi detaylarıyla birlikte getirir.
    /// </summary>
    Task<News?> GetBySlugAsync(string slug, bool includeDrafts = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Etiket benzerliğine göre ilgili haberleri getirir.
    /// </summary>
    Task<List<News>> GetRelatedNewsAsync(Guid currentNewsId, List<int> tagIds, int count = 4, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirli bir zaman aralığındaki en çok okunan haberleri getirir.
    /// </summary>
    Task<List<News>> GetMostViewedNewsAsync(int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Zamanı gelmiş ve otomatik yayına alınması gereken taslakları getirir (Hangfire için).
    /// </summary>
    Task<List<News>> GetPendingScheduledNewsAsync(DateTime nowUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Haberin okunma sayısını doğrudan veritabanında atomik olarak artırır.
    /// </summary>
    Task IncrementViewCountAsync(Guid newsId, int incrementBy = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin paneli için filtreli ve sayfalı haber listesi döner.
    /// </summary>
    Task<(List<News> Items, int TotalCount)> GetAdminPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        NewsStatus? status,
        int? categoryId,
        Guid? authorId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yayın zamanı gelmiş haberleri Published olarak işaretler, etkilenen sayıyı döner.
    /// </summary>
    Task<int> PublishScheduledNewsAsync(CancellationToken cancellationToken = default);
}

