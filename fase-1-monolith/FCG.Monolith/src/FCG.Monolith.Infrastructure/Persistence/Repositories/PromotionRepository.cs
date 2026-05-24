using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FCG.Monolith.Infrastructure.Persistence.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly AppDbContext _context;

    public PromotionRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Promotion>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Promotions
            .Include(p => p.Games)
            .ThenInclude(pg => pg.Game)
            .Where(p => p.StartsAt <= now && p.EndsAt >= now)
            .ToListAsync(ct);
    }

    public async Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Promotions
            .Include(p => p.Games)
            .ThenInclude(pg => pg.Game)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(Promotion promotion, CancellationToken ct = default)
        => await _context.Promotions.AddAsync(promotion, ct);

    public Task DeleteAsync(Promotion promotion, CancellationToken ct = default)
    {
        _context.Promotions.Remove(promotion);
        return Task.CompletedTask;
    }

    public async Task<PromotionGame?> GetPromotionGameAsync(Guid promotionId, Guid gameId, CancellationToken ct = default)
        => await _context.PromotionGames
            .FirstOrDefaultAsync(pg => pg.PromotionId == promotionId && pg.GameId == gameId, ct);

    public async Task AddGameAsync(PromotionGame promotionGame, CancellationToken ct = default)
        => await _context.PromotionGames.AddAsync(promotionGame, ct);

    public Task RemoveGameAsync(PromotionGame promotionGame, CancellationToken ct = default)
    {
        _context.PromotionGames.Remove(promotionGame);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
