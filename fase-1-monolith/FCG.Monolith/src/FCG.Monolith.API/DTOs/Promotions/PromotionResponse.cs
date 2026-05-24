namespace FCG.Monolith.API.DTOs.Promotions;

public record PromotionGameResponse(Guid GameId, string Title, decimal Price, decimal PromotionalPrice);

public record PromotionResponse(
    Guid Id,
    string Title,
    string Description,
    int DiscountPercent,
    DateTime StartsAt,
    DateTime EndsAt,
    DateTime CreatedAt,
    IEnumerable<PromotionGameResponse> Games);
