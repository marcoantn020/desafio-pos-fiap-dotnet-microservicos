using FCG.Monolith.Application.DTOs;

namespace FCG.Monolith.Application.Services;

public interface IGameService
{
    Task<IEnumerable<GameDto>> GetAllAsync(CancellationToken ct = default);
    Task<GameDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<GameDto> CreateAsync(string title, string description, decimal price, string genre, int releaseYear, CancellationToken ct = default);
    Task<GameDto> UpdateAsync(Guid id, string title, string description, decimal price, string genre, int releaseYear, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
