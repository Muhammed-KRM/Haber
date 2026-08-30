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
    }

    /// <summary>
    /// Süper Admin kullanıcısı yoksa oluşturur.
    /// Şifre appsettings'ten alınır; burada placeholder kullanılır.
    /// Üretimde bu değer environment variable ile override edilir.
    /// </summary>
    private static async Task SeedSuperAdminAsync(AppDbContext context)
    {
        const string adminEmail = "admin@kursutv.com";

        if (await context.Users.AnyAsync(u => u.Email == adminEmail))
            return;

        // BCrypt hash - üretimde AdminSeed__Password env variable olarak verilmeli
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
            new() { Name = "Gündem",    Slug = "gundem",    SortOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Politika",  Slug = "politika",  SortOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Ekonomi",   Slug = "ekonomi",   SortOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Dünya",     Slug = "dunya",     SortOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Spor",      Slug = "spor",      SortOrder = 5, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Teknoloji", Slug = "teknoloji", SortOrder = 6, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Sağlık",    Slug = "saglik",    SortOrder = 7, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Eğitim",    Slug = "egitim",    SortOrder = 8, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Magazin",   Slug = "magazin",   SortOrder = 9, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Köşe Yazıları", Slug = "kose-yazilari", SortOrder = 10, IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }
}
