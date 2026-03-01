using Microsoft.EntityFrameworkCore;

namespace NotificationsAPI.Infrastructure.Persistence;

public class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InboxMessage>(b =>
        {
            b.ToTable("InboxMessages");
            b.HasKey(x => x.Id);
            b.Property(x => x.Consumer).HasMaxLength(200);
            b.HasIndex(x => new { x.Id, x.Consumer }).IsUnique();
        });
    }
}