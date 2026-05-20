using FCG.Monolith.Domain.Entities;

namespace FCG.Monolith.Domain.Interfaces;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Game>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Game game, CancellationToken ct = default);
    Task DeleteAsync(Game game, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
