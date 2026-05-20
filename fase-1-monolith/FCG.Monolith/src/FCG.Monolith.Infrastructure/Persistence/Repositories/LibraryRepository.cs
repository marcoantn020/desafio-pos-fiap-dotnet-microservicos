using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FCG.Monolith.Infrastructure.Persistence.Repositories;

public class LibraryRepository : ILibraryRepository
{
    private readonly AppDbContext _context;

    public LibraryRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<LibraryItem>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _context.LibraryItems
            .Include(li => li.Game)
            .Where(li => li.UserId == userId)
            .ToListAsync(ct);

    public Task<LibraryItem?> GetAsync(Guid userId, Guid gameId, CancellationToken ct = default)
        => _context.LibraryItems
            .FirstOrDefaultAsync(li => li.UserId == userId && li.GameId == gameId, ct);

    public async Task AddAsync(LibraryItem item, CancellationToken ct = default)
        => await _context.LibraryItems.AddAsync(item, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
