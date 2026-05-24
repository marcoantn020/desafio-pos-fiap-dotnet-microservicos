using FCG.Monolith.Domain.Entities;

namespace FCG.Monolith.Domain.Interfaces;

public interface IPromotionRepository
{
    Task<IEnumerable<Promotion>> GetAllActiveAsync(CancellationToken ct = default);
    Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Promotion promotion, CancellationToken ct = default);
    Task DeleteAsync(Promotion promotion, CancellationToken ct = default);
    Task<PromotionGame?> GetPromotionGameAsync(Guid promotionId, Guid gameId, CancellationToken ct = default);
    Task AddGameAsync(PromotionGame promotionGame, CancellationToken ct = default);
    Task RemoveGameAsync(PromotionGame promotionGame, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
