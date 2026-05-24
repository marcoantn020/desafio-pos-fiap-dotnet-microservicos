using FCG.Monolith.Application.DTOs;

namespace FCG.Monolith.Application.Services;

public interface ILibraryService
{
    Task<IEnumerable<LibraryItemDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<LibraryItemDto> AddToLibraryAsync(Guid userId, Guid gameId, CancellationToken ct = default);
}
