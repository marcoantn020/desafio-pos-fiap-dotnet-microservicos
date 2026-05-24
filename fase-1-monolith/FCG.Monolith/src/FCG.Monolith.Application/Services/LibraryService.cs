using FCG.Monolith.Application.DTOs;
using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;

namespace FCG.Monolith.Application.Services;

public class LibraryService : ILibraryService
{
    private readonly ILibraryRepository _libraryRepository;
    private readonly IGameRepository _gameRepository;

    public LibraryService(ILibraryRepository libraryRepository, IGameRepository gameRepository)
    {
        _libraryRepository = libraryRepository;
        _gameRepository = gameRepository;
    }

    public async Task<IEnumerable<LibraryItemDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var items = await _libraryRepository.GetByUserIdAsync(userId, ct);
        return items.Select(li => new LibraryItemDto(li.GameId, li.Game.Title, li.Game.Genre, li.Game.Price, li.AcquiredAt));
    }

    public async Task<LibraryItemDto> AddToLibraryAsync(Guid userId, Guid gameId, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, ct)
            ?? throw new KeyNotFoundException("Game not found.");

        var existing = await _libraryRepository.GetAsync(userId, gameId, ct);
        if (existing is not null)
            throw new InvalidOperationException("Game already in library.");

        var item = LibraryItem.Create(userId, gameId);
        await _libraryRepository.AddAsync(item, ct);
        await _libraryRepository.SaveChangesAsync(ct);

        return new LibraryItemDto(gameId, game.Title, game.Genre, game.Price, item.AcquiredAt);
    }
}
