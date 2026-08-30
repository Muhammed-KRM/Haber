using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KursuTV.Data.Entities;

namespace KursuTV.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(100);
        
        // CÃ¼zdan bakiyesi negatif olamaz tarzÄ± constraint'ler database bazlÄ± eklenebilir,
        // ÅŸimdilik EF bazÄ±nda standart bÄ±rakÄ±yoruz. Concurrency iÃ§in lock eklenebilir.
        builder.Property(u => u.TokenBalance).HasDefaultValue(0);
    }
}
