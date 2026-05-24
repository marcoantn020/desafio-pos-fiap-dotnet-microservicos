namespace FCG.Monolith.Domain.Entities;

public class PromotionGame
{
    public Guid PromotionId { get; private set; }
    public Guid GameId { get; private set; }
    public DateTime AddedAt { get; private set; }
    public Promotion Promotion { get; private set; } = null!;
    public Game Game { get; private set; } = null!;

    private PromotionGame() { }

    public static PromotionGame Create(Guid promotionId, Guid gameId)
    {
        if (promotionId == Guid.Empty) throw new ArgumentException("PromotionId is required.", nameof(promotionId));
        if (gameId == Guid.Empty) throw new ArgumentException("GameId is required.", nameof(gameId));

        return new PromotionGame
        {
            PromotionId = promotionId,
            GameId = gameId,
            AddedAt = DateTime.UtcNow
        };
    }
}
