using KursuTV.Data.Entities;
using KursuTV.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KursuTV.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(120);

        builder.HasIndex(t => t.Slug).IsUnique();
    }
}

public class NewsTagConfiguration : IEntityTypeConfiguration<NewsTag>
{
    public void Configure(EntityTypeBuilder<NewsTag> builder)
    {
        builder.HasKey(nt => new { nt.NewsId, nt.TagId });

        builder.HasOne(nt => nt.News)
            .WithMany(n => n.NewsTags)
            .HasForeignKey(nt => nt.NewsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(nt => nt.Tag)
            .WithMany(t => t.NewsTags)
            .HasForeignKey(nt => nt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MediaConfiguration : IEntityTypeConfiguration<Media>
{
    public void Configure(EntityTypeBuilder<Media> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.OriginalUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(m => m.ThumbnailUrl)
            .HasMaxLength(2048);

        builder.Property(m => m.ListUrl)
            .HasMaxLength(2048);

        builder.Property(m => m.AltText)
            .HasMaxLength(300);

        builder.Property(m => m.MimeType)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(m => m.News)
            .WithMany(n => n.MediaItems)
            .HasForeignKey(m => m.NewsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.UploadedByUser)
            .WithMany()
            .HasForeignKey(m => m.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.GuestName)
            .HasMaxLength(100);

        builder.Property(c => c.Status)
            .HasDefaultValue(CommentStatus.Pending)
            .HasConversion<int>();

        // Onay bekleyen yorumları hızlı çekmek için index
        builder.HasIndex(c => c.Status);

        builder.HasOne(c => c.News)
            .WithMany(n => n.Comments)
            .HasForeignKey(c => c.NewsId)
            .OnDelete(DeleteBehavior.Cascade);

        // Öz-referanslı yanıt ilişkisi
        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
