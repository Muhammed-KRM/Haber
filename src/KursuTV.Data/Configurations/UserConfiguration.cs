using KursuTV.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KursuTV.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Bio)
            .HasMaxLength(1000);

        builder.Property(u => u.ProfileImageUrl)
            .HasMaxLength(2048);

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(512);

        builder.Property(u => u.BanReason)
            .HasMaxLength(500);

        // News ilişkisi: Kullanıcı silinince haberler silinmez, AuthorId null olur
        builder.HasMany(u => u.News)
            .WithOne(n => n.Author)
            .HasForeignKey(n => n.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Comments)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
