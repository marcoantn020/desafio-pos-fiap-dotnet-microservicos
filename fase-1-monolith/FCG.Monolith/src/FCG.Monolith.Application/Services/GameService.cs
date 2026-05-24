using FCG.Monolith.Application.DTOs;
using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;

namespace FCG.Monolith.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IPromotionRepository _promotionRepository;

    public GameService(IGameRepository gameRepository, IPromotionRepository promotionRepository)
    {
        _gameRepository = gameRepository;
        _promotionRepository = promotionRepository;
    }

    public async Task<IEnumerable<GameDto>> GetAllAsync(CancellationToken ct = default)
    {
        var games = await _gameRepository.GetAllAsync(ct);
        var activePromotions = (await _promotionRepository.GetAllActiveAsync(ct)).ToList();
        return games.Select(g => ToDto(g, activePromotions));
    }

    public async Task<GameDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(id, ct);
        if (game is null) return null;
        var activePromotions = (await _promotionRepository.GetAllActiveAsync(ct)).ToList();
        return ToDto(game, activePromotions);
    }

    public async Task<GameDto> CreateAsync(string title, string description, decimal price, string genre, int releaseYear, CancellationToken ct = default)
    {
        var game = Game.Create(title, description, price, genre, releaseYear);
        await _gameRepository.AddAsync(game, ct);
        await _gameRepository.SaveChangesAsync(ct);
        return new GameDto(game.Id, game.Title, game.Description, game.Price, game.Genre, game.ReleaseYear, game.CreatedAt, null);
    }

    public async Task<GameDto> UpdateAsync(Guid id, string title, string description, decimal price, string genre, int releaseYear, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Game {id} not found.");
        game.Update(title, description, price, genre, releaseYear);
        await _gameRepository.SaveChangesAsync(ct);
        var activePromotions = (await _promotionRepository.GetAllActiveAsync(ct)).ToList();
        return ToDto(game, activePromotions);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Game {id} not found.");
        await _gameRepository.DeleteAsync(game, ct);
        await _gameRepository.SaveChangesAsync(ct);
    }

    private static GameDto ToDto(Game game, List<Promotion> activePromotions)
    {
        var bestDiscount = activePromotions
            .Where(p => p.Games.Any(pg => pg.GameId == game.Id))
            .Select(p => p.DiscountPercent)
            .DefaultIfEmpty(0)
            .Max();

        decimal? promotionalPrice = bestDiscount > 0
            ? Math.Round(game.Price * (1 - bestDiscount / 100m), 2)
            : null;

        return new GameDto(game.Id, game.Title, game.Description, game.Price,
            game.Genre, game.ReleaseYear, game.CreatedAt, promotionalPrice);
    }
}
