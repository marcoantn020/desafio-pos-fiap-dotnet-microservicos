using FCG.Monolith.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Monolith.Infrastructure.Persistence.Configurations;

public class PromotionGameConfiguration : IEntityTypeConfiguration<PromotionGame>
{
    public void Configure(EntityTypeBuilder<PromotionGame> builder)
    {
        builder.HasKey(pg => new { pg.PromotionId, pg.GameId });
        builder.Property(pg => pg.AddedAt).IsRequired();

        builder.HasOne(pg => pg.Game)
            .WithMany()
            .HasForeignKey(pg => pg.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
