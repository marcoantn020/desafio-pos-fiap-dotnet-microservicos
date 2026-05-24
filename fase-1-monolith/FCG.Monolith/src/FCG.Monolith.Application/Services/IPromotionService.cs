using FCG.Monolith.Application.DTOs;

namespace FCG.Monolith.Application.Services;

public interface IPromotionService
{
    Task<IEnumerable<PromotionDto>> GetAllActiveAsync(CancellationToken ct = default);
    Task<PromotionDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PromotionDto> CreateAsync(string title, string description, int discountPercent, DateTime startsAt, DateTime endsAt, CancellationToken ct = default);
    Task<PromotionDto> UpdateAsync(Guid id, string title, string description, int discountPercent, DateTime startsAt, DateTime endsAt, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task AddGameAsync(Guid promotionId, Guid gameId, CancellationToken ct = default);
    Task RemoveGameAsync(Guid promotionId, Guid gameId, CancellationToken ct = default);
}
