using KursuTV.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KursuTV.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(120);

        builder.HasIndex(c => c.Slug).IsUnique();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        // Öz-referanslı ilişki (alt kategoriler)
        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class NewsCategoryConfiguration : IEntityTypeConfiguration<NewsCategory>
{
    public void Configure(EntityTypeBuilder<NewsCategory> builder)
    {
        // Bileşik birincil anahtar (pivot tablo)
        builder.HasKey(nc => new { nc.NewsId, nc.CategoryId });

        builder.HasOne(nc => nc.News)
            .WithMany(n => n.NewsCategories)
            .HasForeignKey(nc => nc.NewsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(nc => nc.Category)
            .WithMany(c => c.NewsCategories)
            .HasForeignKey(nc => nc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
