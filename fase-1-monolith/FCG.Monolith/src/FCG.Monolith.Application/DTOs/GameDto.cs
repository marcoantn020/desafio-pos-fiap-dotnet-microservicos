namespace FCG.Monolith.Application.DTOs;

public record GameDto(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    string Genre,
    int ReleaseYear,
    DateTime CreatedAt,
    decimal? PromotionalPrice);
