using FCG.Monolith.API.DTOs.Games;
using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Monolith.API.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly IGameRepository _gameRepository;

    public GamesController(IGameRepository gameRepository) => _gameRepository = gameRepository;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var games = await _gameRepository.GetAllAsync(ct);
        return Ok(games.Select(g => new GameResponse(g.Id, g.Title, g.Description, g.Price, g.Genre, g.ReleaseYear, g.CreatedAt)));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var game = await _gameRepository.GetByIdAsync(id, ct);
        if (game is null) return NotFound();
        return Ok(new GameResponse(game.Id, game.Title, game.Description, game.Price, game.Genre, game.ReleaseYear, game.CreatedAt));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateGameRequest request, CancellationToken ct)
    {
        var game = Game.Create(request.Title, request.Description ?? string.Empty, request.Price, request.Genre ?? string.Empty, request.ReleaseYear);
        await _gameRepository.AddAsync(game, ct);
        await _gameRepository.SaveChangesAsync(ct);
        return Created($"/api/games/{game.Id}",
            new GameResponse(game.Id, game.Title, game.Description, game.Price, game.Genre, game.ReleaseYear, game.CreatedAt));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGameRequest request, CancellationToken ct)
    {
        var game = await _gameRepository.GetByIdAsync(id, ct);
        if (game is null) return NotFound();
        game.Update(request.Title, request.Description ?? string.Empty, request.Price, request.Genre ?? string.Empty, request.ReleaseYear);
        await _gameRepository.SaveChangesAsync(ct);
        return Ok(new GameResponse(game.Id, game.Title, game.Description, game.Price, game.Genre, game.ReleaseYear, game.CreatedAt));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var game = await _gameRepository.GetByIdAsync(id, ct);
        if (game is null) return NotFound();
        await _gameRepository.DeleteAsync(game, ct);
        await _gameRepository.SaveChangesAsync(ct);
        return NoContent();
    }
}
