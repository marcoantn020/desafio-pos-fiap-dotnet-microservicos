using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FCG.Monolith.Infrastructure.Persistence.Repositories;

public class GameRepository : IGameRepository
{
    private readonly AppDbContext _context;

    public GameRepository(AppDbContext context) => _context = context;

    public Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Games.FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<IEnumerable<Game>> GetAllAsync(CancellationToken ct = default)
        => await _context.Games.OrderBy(g => g.Title).ToListAsync(ct);

    public async Task AddAsync(Game game, CancellationToken ct = default)
        => await _context.Games.AddAsync(game, ct);

    public Task DeleteAsync(Game game, CancellationToken ct = default)
    {
        _context.Games.Remove(game);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
