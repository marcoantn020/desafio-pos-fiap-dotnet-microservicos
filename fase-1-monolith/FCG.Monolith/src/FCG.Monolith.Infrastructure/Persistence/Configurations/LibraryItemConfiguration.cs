using FCG.Monolith.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Monolith.Infrastructure.Persistence.Configurations;

public class LibraryItemConfiguration : IEntityTypeConfiguration<LibraryItem>
{
    public void Configure(EntityTypeBuilder<LibraryItem> builder)
    {
        builder.HasKey(li => new { li.UserId, li.GameId });

        builder.HasOne(li => li.User)
            .WithMany(u => u.Library)
            .HasForeignKey(li => li.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(li => li.Game)
            .WithMany()
            .HasForeignKey(li => li.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(li => li.AcquiredAt).IsRequired();
    }
}
