namespace FCG.Monolith.API.DTOs.Library;

public record LibraryItemResponse(
    Guid GameId,
    string Title,
    string Genre,
    decimal Price,
    DateTime AcquiredAt
);
