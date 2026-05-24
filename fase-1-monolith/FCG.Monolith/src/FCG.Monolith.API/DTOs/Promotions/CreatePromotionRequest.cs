namespace FCG.Monolith.API.DTOs.Promotions;

public record CreatePromotionRequest(
    string Title,
    string? Description,
    int DiscountPercent,
    DateTime StartsAt,
    DateTime EndsAt);
