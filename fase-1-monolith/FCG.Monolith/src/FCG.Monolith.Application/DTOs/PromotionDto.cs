namespace FCG.Monolith.Application.DTOs;

public record PromotionGameDto(Guid GameId, string Title, decimal Price, decimal PromotionalPrice);

public record PromotionDto(
    Guid Id,
    string Title,
    string Description,
    int DiscountPercent,
    DateTime StartsAt,
    DateTime EndsAt,
    DateTime CreatedAt,
    IEnumerable<PromotionGameDto> Games);
