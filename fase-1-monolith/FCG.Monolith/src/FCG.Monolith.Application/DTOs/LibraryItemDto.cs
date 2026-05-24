namespace FCG.Monolith.Application.DTOs;

public record LibraryItemDto(
    Guid GameId,
    string Title,
    string Genre,
    decimal Price,
    DateTime AcquiredAt);
