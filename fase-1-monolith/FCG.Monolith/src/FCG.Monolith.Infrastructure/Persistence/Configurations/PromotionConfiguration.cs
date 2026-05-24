using FCG.Monolith.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Monolith.Infrastructure.Persistence.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.DiscountPercent).IsRequired();
        builder.Property(p => p.StartsAt).IsRequired();
        builder.Property(p => p.EndsAt).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasMany(p => p.Games)
            .WithOne(pg => pg.Promotion)
            .HasForeignKey(pg => pg.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
