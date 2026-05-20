using FCG.Monolith.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Monolith.Infrastructure.Persistence.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Title).IsRequired().HasMaxLength(200);
        builder.HasIndex(g => g.Title).IsUnique();
        builder.Property(g => g.Description).HasMaxLength(2000);
        builder.Property(g => g.Price).IsRequired().HasColumnType("decimal(10,2)");
        builder.Property(g => g.Genre).HasMaxLength(100);
        builder.Property(g => g.ReleaseYear).IsRequired();
        builder.Property(g => g.CreatedAt).IsRequired();
    }
}
