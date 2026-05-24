using FCG.Monolith.Application.DTOs;
using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;

namespace FCG.Monolith.Application.Services;

public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IGameRepository _gameRepository;

    public PromotionService(IPromotionRepository promotionRepository, IGameRepository gameRepository)
    {
        _promotionRepository = promotionRepository;
        _gameRepository = gameRepository;
    }

    public async Task<IEnumerable<PromotionDto>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var promotions = await _promotionRepository.GetAllActiveAsync(ct);
        return promotions.Select(ToDto);
    }

    public async Task<PromotionDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var promotion = await _promotionRepository.GetByIdAsync(id, ct);
        return promotion is null ? null : ToDto(promotion);
    }

    public async Task<PromotionDto> CreateAsync(string title, string description, int discountPercent,
        DateTime startsAt, DateTime endsAt, CancellationToken ct = default)
    {
        var promotion = Promotion.Create(title, description, discountPercent, startsAt, endsAt);
        await _promotionRepository.AddAsync(promotion, ct);
        await _promotionRepository.SaveChangesAsync(ct);
        return ToDto(promotion);
    }

    public async Task<PromotionDto> UpdateAsync(Guid id, string title, string description, int discountPercent,
        DateTime startsAt, DateTime endsAt, CancellationToken ct = default)
    {
        var promotion = await _promotionRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Promotion {id} not found.");
        promotion.Update(title, description, discountPercent, startsAt, endsAt);
        await _promotionRepository.SaveChangesAsync(ct);
        return ToDto(promotion);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var promotion = await _promotionRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Promotion {id} not found.");
        await _promotionRepository.DeleteAsync(promotion, ct);
        await _promotionRepository.SaveChangesAsync(ct);
    }

    public async Task AddGameAsync(Guid promotionId, Guid gameId, CancellationToken ct = default)
    {
        _ = await _promotionRepository.GetByIdAsync(promotionId, ct)
            ?? throw new KeyNotFoundException($"Promotion {promotionId} not found.");
        _ = await _gameRepository.GetByIdAsync(gameId, ct)
            ?? throw new KeyNotFoundException($"Game {gameId} not found.");

        var existing = await _promotionRepository.GetPromotionGameAsync(promotionId, gameId, ct);
        if (existing is not null)
            throw new InvalidOperationException("Game already in promotion.");

        var pg = PromotionGame.Create(promotionId, gameId);
        await _promotionRepository.AddGameAsync(pg, ct);
        await _promotionRepository.SaveChangesAsync(ct);
    }

    public async Task RemoveGameAsync(Guid promotionId, Guid gameId, CancellationToken ct = default)
    {
        var pg = await _promotionRepository.GetPromotionGameAsync(promotionId, gameId, ct)
            ?? throw new KeyNotFoundException("Game not in promotion.");
        await _promotionRepository.RemoveGameAsync(pg, ct);
        await _promotionRepository.SaveChangesAsync(ct);
    }

    private static PromotionDto ToDto(Promotion p)
    {
        var games = p.Games.Select(pg => new PromotionGameDto(
            pg.GameId,
            pg.Game?.Title ?? string.Empty,
            pg.Game?.Price ?? 0,
            Math.Round((pg.Game?.Price ?? 0) * (1 - p.DiscountPercent / 100m), 2)));
        return new PromotionDto(p.Id, p.Title, p.Description, p.DiscountPercent,
            p.StartsAt, p.EndsAt, p.CreatedAt, games);
    }
}
