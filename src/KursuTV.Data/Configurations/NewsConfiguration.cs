using KursuTV.Data.Entities;
using KursuTV.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KursuTV.Data.Configurations;

public class NewsConfiguration : IEntityTypeConfiguration<News>
{
    public void Configure(EntityTypeBuilder<News> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(n => n.Slug)
            .IsRequired()
            .HasMaxLength(350);

        // Slug benzersiz olmalı (SEO için kritik)
        builder.HasIndex(n => n.Slug)
            .IsUnique();

        builder.Property(n => n.Spot)
            .HasMaxLength(500);

        // HTML içerik - sınırsız metin (TEXT tipi)
        builder.Property(n => n.Content)
            .IsRequired();

        builder.Property(n => n.CoverImageUrl)
            .HasMaxLength(2048);

        builder.Property(n => n.CoverImageAlt)
            .HasMaxLength(300);

        builder.Property(n => n.MetaTitle)
            .HasMaxLength(160);

        builder.Property(n => n.MetaDescription)
            .HasMaxLength(320);

        builder.Property(n => n.Status)
            .HasDefaultValue(NewsStatus.Draft)
            .HasConversion<int>();

        builder.Property(n => n.Type)
            .HasDefaultValue(NewsType.Article)
            .HasConversion<int>();

        // Yayınlanmış haberler için bileşik index: performanslı listeleme
        builder.HasIndex(n => new { n.Status, n.PublishedAt });

        // Son dakika bandı için index
        builder.HasIndex(n => n.IsBreaking);

        // Manşet sıralaması için index
        builder.HasIndex(n => n.HeadlineOrder);

        // Author ilişkisi
        builder.HasOne(n => n.Author)
            .WithMany(u => u.News)
            .HasForeignKey(n => n.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
