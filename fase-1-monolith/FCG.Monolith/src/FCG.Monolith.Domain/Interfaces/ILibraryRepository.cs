using FCG.Monolith.Domain.Entities;

namespace FCG.Monolith.Domain.Interfaces;

public interface ILibraryRepository
{
    Task<IEnumerable<LibraryItem>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<LibraryItem?> GetAsync(Guid userId, Guid gameId, CancellationToken ct = default);
    Task AddAsync(LibraryItem item, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
