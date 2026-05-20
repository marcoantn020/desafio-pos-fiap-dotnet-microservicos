namespace FCG.Monolith.Domain.Entities;

public class LibraryItem
{
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public DateTime AcquiredAt { get; private set; }
    public User User { get; private set; } = null!;
    public Game Game { get; private set; } = null!;

    private LibraryItem() { }

    public static LibraryItem Create(Guid userId, Guid gameId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (gameId == Guid.Empty) throw new ArgumentException("GameId is required.", nameof(gameId));
        return new LibraryItem
        {
            UserId = userId,
            GameId = gameId,
            AcquiredAt = DateTime.UtcNow
        };
    }
}
