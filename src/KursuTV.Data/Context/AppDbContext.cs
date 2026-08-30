using Microsoft.EntityFrameworkCore;
using KursuTV.Data.Entities;

namespace KursuTV.Data.Context;

/// <summary>
/// Uygulamanın ana veritabanı bağlamı.
/// Tüm entity konfigürasyonları Configurations/ klasöründeki
/// IEntityTypeConfiguration implementasyonlarından otomatik yüklenir.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // --- Kullanıcı ve Kimlik ---
    public DbSet<User> Users => Set<User>();

    // --- Haber Çekirdeği ---
    public DbSet<News> News => Set<News>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Media> MediaItems => Set<Media>();
    public DbSet<Comment> Comments => Set<Comment>();

    // --- Pivot Tablolar ---
    public DbSet<NewsCategory> NewsCategories => Set<NewsCategory>();
    public DbSet<NewsTag> NewsTags => Set<NewsTag>();

    // --- Loglama (Serilog Sinks) ---
    public DbSet<GlobalSetting> GlobalSettings => Set<GlobalSetting>();
    public DbSet<EndpointLog> EndpointLogs => Set<EndpointLog>();
    public DbSet<FunctionLog> FunctionLogs => Set<FunctionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurations/ altındaki IEntityTypeConfiguration sınıflarını otomatik tara ve uygula
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
