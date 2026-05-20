using System.Security.Claims;
using FCG.Monolith.API.DTOs.Library;
using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Monolith.API.Controllers;

[ApiController]
[Route("api/library")]
[Authorize]
public class LibraryController : ControllerBase
{
    private readonly ILibraryRepository _libraryRepository;
    private readonly IGameRepository _gameRepository;

    public LibraryController(ILibraryRepository libraryRepository, IGameRepository gameRepository)
    {
        _libraryRepository = libraryRepository;
        _gameRepository = gameRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyLibrary(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var items = await _libraryRepository.GetByUserIdAsync(userId, ct);
        return Ok(items.Select(li => new LibraryItemResponse(li.GameId, li.Game.Title, li.Game.Genre, li.Game.Price, li.AcquiredAt)));
    }

    [HttpPost("{gameId:guid}")]
    public async Task<IActionResult> AddToLibrary(Guid gameId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var game = await _gameRepository.GetByIdAsync(gameId, ct);
        if (game is null) return NotFound(new { error = "Game not found." });

        var existing = await _libraryRepository.GetAsync(userId, gameId, ct);
        if (existing is not null) return Conflict(new { error = "Game already in library." });

        var item = LibraryItem.Create(userId, gameId);
        await _libraryRepository.AddAsync(item, ct);
        await _libraryRepository.SaveChangesAsync(ct);

        return Created("/api/library",
            new LibraryItemResponse(gameId, game.Title, game.Genre, game.Price, item.AcquiredAt));
    }
}
