namespace FCG.Monolith.API.DTOs.Promotions;

public record UpdatePromotionRequest(
    string Title,
    string? Description,
    int DiscountPercent,
    DateTime StartsAt,
    DateTime EndsAt);
