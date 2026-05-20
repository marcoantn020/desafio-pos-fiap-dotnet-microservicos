namespace FCG.Monolith.API.DTOs.Games;

public record GameResponse(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    string Genre,
    int ReleaseYear,
    DateTime CreatedAt
);
