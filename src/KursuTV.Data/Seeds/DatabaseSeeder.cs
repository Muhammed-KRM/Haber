using KursuTV.Data.Context;
using KursuTV.Data.Entities;
using KursuTV.Data.Enums;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace KursuTV.Data.Seeds;

/// <summary>
/// Uygulama ilk kurulumunda zorunlu varsayılan verileri yükler.
/// Her çalıştırmada idempotent davranır (varsa tekrar eklemez).
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedSuperAdminAsync(context);
        await SeedDefaultCategoriesAsync(context);
        await SeedDefaultAuthorsAndNewsAsync(context);
    }

    /// <summary>
    /// Süper Admin kullanıcısı yoksa oluşturur.
    /// </summary>
    private static async Task SeedSuperAdminAsync(AppDbContext context)
    {
        const string adminEmail = "admin@kursutv.com";

        if (await context.Users.AnyAsync(u => u.Email == adminEmail))
            return;

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123!"),
            FullName = "Sistem Yöneticisi",
            Role = UserRole.SuperAdmin,
            IsActive = true,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Temel haber kategorilerini oluşturur (Gündem, Spor, Ekonomi vb.).
    /// </summary>
    private static async Task SeedDefaultCategoriesAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        var categories = new List<Category>
        {
            new() { Name = "Gündem",        Slug = "gundem",        SortOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Politika",      Slug = "politika",      SortOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Ekonomi",       Slug = "ekonomi",       SortOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Dünya",         Slug = "dunya",         SortOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Spor",          Slug = "spor",          SortOrder = 5, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Kültür & Sanat",Slug = "kultur-sanat",  SortOrder = 6, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Teknoloji",     Slug = "teknoloji",     SortOrder = 7, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Röportaj",      Slug = "roportaj",      SortOrder = 8, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Sağlık",        Slug = "saglik",        SortOrder = 9, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Köşe Yazıları", Slug = "kose-yazilari", SortOrder = 10, IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Örnek köşe yazarları, köşe yazıları ve video haberleri tohumlar.
    /// </summary>
    private static async Task SeedDefaultAuthorsAndNewsAsync(AppDbContext context)
    {
        if (await context.News.AnyAsync())
            return;

        // 1. Köşe Yazarları
        var author1 = new User
        {
            Id = Guid.NewGuid(),
            Email = "cevdet.sezgin@kursutv.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Yazar@123!"),
            FullName = "Cevdet Sezgin",
            Role = UserRole.ColumnWriter,
            Bio = "Başyazar / Ekonomi & Politika",
            ProfileImageUrl = "/images/authors/author-1.png",
            IsActive = true,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        var author2 = new User
        {
            Id = Guid.NewGuid(),
            Email = "selin.yilmaz@kursutv.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Yazar@123!"),
            FullName = "Dr. Selin Yılmaz",
            Role = UserRole.ColumnWriter,
            Bio = "Ekonomist & Finans Analisti",
            ProfileImageUrl = "/images/authors/author-2.png",
            IsActive = true,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        var author3 = new User
        {
            Id = Guid.NewGuid(),
            Email = "murat.guler@kursutv.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Yazar@123!"),
            FullName = "Murat Güler",
            Role = UserRole.ColumnWriter,
            Bio = "Dış Politika & Strateji Uzmanı",
            ProfileImageUrl = "/images/authors/author-3.png",
            IsActive = true,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        var author4 = new User
        {
            Id = Guid.NewGuid(),
            Email = "zeynep.kaya@kursutv.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Yazar@123!"),
            FullName = "Zeynep Kaya",
            Role = UserRole.ColumnWriter,
            Bio = "Kültür & Edebiyat Yazarı",
            ProfileImageUrl = "/images/authors/author-4.png",
            IsActive = true,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Users.AddRangeAsync(author1, author2, author3, author4);
        await context.SaveChangesAsync();

        var gundemCat = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "gundem");
        var ekonomiCat = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "ekonomi");
        var dunyaCat = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "dunya");
        var kulturCat = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "kultur-sanat");
        var koseCat = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "kose-yazilari");

        var now = DateTime.UtcNow;

        // 2. Köşe Yazıları (NewsType.Column)
        var columnNews1 = new News
        {
            Id = Guid.NewGuid(),
            Title = "Bal Gibi Pazarlık Yapmışsınız",
            Slug = "bal-gibi-pazarlik-yapmissiniz",
            Spot = "Son günlerde yaşanan ekonomik ve siyasi gelişmelerin arka planındaki müzakereler.",
            Content = "<p>Son günlerde yaşanan ekonomik ve siyasi gelişmeler, arka planda yürütülen müzakerelerin ne kadar stratejik olduğunu bir kez daha gözler önüne seriyor. Piyasa dinamikleri ve kamu politikaları arasındaki denge yeniden kuruluyor.</p>",
            Type = NewsType.Column,
            Status = NewsStatus.Published,
            PublishedAt = now,
            CreatedAt = now,
            AuthorId = author1.Id,
            CoverImageUrl = author1.ProfileImageUrl
        };

        var columnNews2 = new News
        {
            Id = Guid.NewGuid(),
            Title = "Yeni Ekonomik Dengeler ve Beklentiler",
            Slug = "yeni-ekonomik-dengeler-ve-beklentiler",
            Spot = "Küresel piyasaların seyrinde enflasyon beklentileri ve faiz politikaları.",
            Content = "<p>Merkez bankalarının aldığı kararlar ve küresel ticaret koridorlarındaki değişimler, orta vadeli ekonomik tahminleri yeniden şekillendiriyor.</p>",
            Type = NewsType.Column,
            Status = NewsStatus.Published,
            PublishedAt = now.AddDays(-1),
            CreatedAt = now.AddDays(-1),
            AuthorId = author2.Id,
            CoverImageUrl = author2.ProfileImageUrl
        };

        var columnNews3 = new News
        {
            Id = Guid.NewGuid(),
            Title = "Ortadoğu'da Değişen Dengeler ve Diplomasi",
            Slug = "ortadoguda-degisen-dengeler",
            Spot = "Bölgesel ittifaklar ve enerji koridorları üzerindeki yeni jeopolitik satranç.",
            Content = "<p>Bölgede kurulan yeni ittifaklar ve enerji hatları güvenliği, diplomasi masasında belirleyici aktörlerin manevra alanını belirliyor.</p>",
            Type = NewsType.Column,
            Status = NewsStatus.Published,
            PublishedAt = now.AddDays(-2),
            CreatedAt = now.AddDays(-2),
            AuthorId = author3.Id,
            CoverImageUrl = author3.ProfileImageUrl
        };

        var columnNews4 = new News
        {
            Id = Guid.NewGuid(),
            Title = "Kültür Dünyamızda Neler Oluyor?",
            Slug = "kultur-dunyamizda-neler-oluyor",
            Spot = "Sanatın ve edebiyatın toplumsal hafızadaki yeri ve dönüşümü.",
            Content = "<p>Modern çağın dijitalleşen dünyasında edebiyat ve sanat, insanlığın ortak hafızasını koruma görevini her zamankinden daha güçlü biçimde üstleniyor.</p>",
            Type = NewsType.Column,
            Status = NewsStatus.Published,
            PublishedAt = now.AddDays(-3),
            CreatedAt = now.AddDays(-3),
            AuthorId = author4.Id,
            CoverImageUrl = author4.ProfileImageUrl
        };

        // 3. Video Haberler (NewsType.Video)
        var videoNews1 = new News
        {
            Id = Guid.NewGuid(),
            Title = "Kürsü TV Gündem Özel: Haftanın Analizi",
            Slug = "kursu-tv-gundem-ozel-haftanin-analizi",
            Spot = "Haftanın en çok konuşulan başlıkları stüdyo konuklarıyla masaya yatırılıyor.",
            Content = "<p>Gündem Özel programında bu hafta, siyaset ve ekonomideki en sıcak gelişmeler uzman konuklarla birlikte canlı yayında değerlendirildi.</p>",
            Type = NewsType.Video,
            Status = NewsStatus.Published,
            PublishedAt = now,
            CreatedAt = now,
            AuthorId = author1.Id,
            CoverImageUrl = "/images/default-news.png"
        };

        var videoNews2 = new News
        {
            Id = Guid.NewGuid(),
            Title = "Özel Röportaj: Bağımsızlık ve Cumhuriyetin Kazanımları",
            Slug = "ozel-roportaj-bagimsizlik-ve-cumhuriyet",
            Spot = "Tarihçilerle milli mücadelenin bilinmeyen yönleri ve arşiv belgeleri.",
            Content = "<p>Cumhuriyetimizin kuruluş sürecindeki kritik kararlar ve belgelerin ışığında tarihi yolculuk Kürsü TV ekranlarında.</p>",
            Type = NewsType.Video,
            Status = NewsStatus.Published,
            PublishedAt = now.AddHours(-6),
            CreatedAt = now.AddHours(-6),
            AuthorId = author3.Id,
            CoverImageUrl = "/images/default-news.png"
        };

        // 4. Manşet Haberleri (HeadlineOrder 1, 2, 3)
        var headlineNews1 = new News
        {
            Id = Guid.NewGuid(),
            Title = "Türkiye Genelinde Ekonomik Reform Paketi Açıklandı",
            Slug = "turkiye-genelinde-ekonomik-reform-paketi",
            Spot = "Üretim ve ihracat odaklı yeni teşvik modeli kamuoyuyla paylaşıldı.",
            Content = "<p>Açıklanan yeni ekonomik reform paketinde KOBİ'lere yönelik düşük faizli kredi imkanları, vergi muafiyetleri ve teknoloji yatırımlarına özel teşvikler yer aldı.</p>",
            Type = NewsType.Article,
            Status = NewsStatus.Published,
            HeadlineOrder = 1,
            IsBreaking = true,
            PublishedAt = now,
            CreatedAt = now,
            AuthorId = author1.Id,
            CoverImageUrl = "/images/default-news.png"
        };

        var headlineNews2 = new News
        {
            Id = Guid.NewGuid(),
            Title = "Küresel İklim Zirvesinde Tarihi Mutabakat İmzalandı",
            Slug = "kuresel-iklim-zirvesinde-tarihi-mutabakat",
            Spot = "Karbon salınımını azaltma konusunda dünya liderleri ortak taahhütte bulundu.",
            Content = "<p>Zirveye katılan 140'tan fazla ülke, 2030 yılına kadar yenilenebilir enerji kapasitesini üç katına çıkarma kararı aldı.</p>",
            Type = NewsType.Article,
            Status = NewsStatus.Published,
            HeadlineOrder = 2,
            PublishedAt = now.AddHours(-3),
            CreatedAt = now.AddHours(-3),
            AuthorId = author3.Id,
            CoverImageUrl = "/images/default-news.png"
        };

        var headlineNews3 = new News
        {
            Id = Guid.NewGuid(),
            Title = "Milli Teknoloji Hamlesinde Yeni Nesil Uydu Fırlatıldı",
            Slug = "milli-teknoloji-hamlesinde-yeni-nesil-uydu",
            Spot = "Yerli mühendislik ürünü haberleşme uydusu başarıyla yörüngesine yerleşti.",
            Content = "<p>Uzay ajansı yetkilileri, uydudan ilk sinyallerin başarıyla alındığını ve tüm sistemlerin nominal çalıştığını bildirdi.</p>",
            Type = NewsType.Article,
            Status = NewsStatus.Published,
            HeadlineOrder = 3,
            IsBreaking = true,
            PublishedAt = now.AddHours(-5),
            CreatedAt = now.AddHours(-5),
            AuthorId = author2.Id,
            CoverImageUrl = "/images/default-news.png"
        };

        var allNews = new List<News>
        {
            columnNews1, columnNews2, columnNews3, columnNews4,
            videoNews1, videoNews2,
            headlineNews1, headlineNews2, headlineNews3
        };

        await context.News.AddRangeAsync(allNews);

        // Kategori İlişkileri
        if (koseCat != null)
        {
            await context.Set<NewsCategory>().AddRangeAsync(
                new NewsCategory { NewsId = columnNews1.Id, CategoryId = koseCat.Id },
                new NewsCategory { NewsId = columnNews2.Id, CategoryId = koseCat.Id },
                new NewsCategory { NewsId = columnNews3.Id, CategoryId = koseCat.Id },
                new NewsCategory { NewsId = columnNews4.Id, CategoryId = koseCat.Id }
            );
        }

        if (gundemCat != null)
        {
            await context.Set<NewsCategory>().AddRangeAsync(
                new NewsCategory { NewsId = videoNews1.Id, CategoryId = gundemCat.Id },
                new NewsCategory { NewsId = videoNews2.Id, CategoryId = gundemCat.Id },
                new NewsCategory { NewsId = headlineNews1.Id, CategoryId = gundemCat.Id }
            );
        }

        if (ekonomiCat != null)
        {
            await context.Set<NewsCategory>().AddAsync(
                new NewsCategory { NewsId = headlineNews1.Id, CategoryId = ekonomiCat.Id }
            );
        }

        if (dunyaCat != null)
        {
            await context.Set<NewsCategory>().AddAsync(
                new NewsCategory { NewsId = headlineNews2.Id, CategoryId = dunyaCat.Id }
            );
        }

        if (kulturCat != null)
        {
            await context.Set<NewsCategory>().AddAsync(
                new NewsCategory { NewsId = columnNews4.Id, CategoryId = kulturCat.Id }
            );
        }

        await context.SaveChangesAsync();
    }
}
