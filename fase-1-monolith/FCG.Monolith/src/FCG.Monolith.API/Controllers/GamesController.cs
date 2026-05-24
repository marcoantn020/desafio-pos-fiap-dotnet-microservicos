using FCG.Monolith.API.DTOs.Games;
using FCG.Monolith.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Monolith.API.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService) => _gameService = gameService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var games = await _gameService.GetAllAsync(ct);
        return Ok(games.Select(g => new GameResponse(g.Id, g.Title, g.Description, g.Price, g.Genre, g.ReleaseYear, g.CreatedAt, g.PromotionalPrice)));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var game = await _gameService.GetByIdAsync(id, ct);
        if (game is null) return NotFound();
        return Ok(new GameResponse(game.Id, game.Title, game.Description, game.Price, game.Genre, game.ReleaseYear, game.CreatedAt, game.PromotionalPrice));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateGameRequest request, CancellationToken ct)
    {
        var game = await _gameService.CreateAsync(request.Title, request.Description ?? string.Empty,
            request.Price, request.Genre ?? string.Empty, request.ReleaseYear, ct);
        return Created($"/api/games/{game.Id}",
            new GameResponse(game.Id, game.Title, game.Description, game.Price, game.Genre, game.ReleaseYear, game.CreatedAt, game.PromotionalPrice));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGameRequest request, CancellationToken ct)
    {
        var game = await _gameService.UpdateAsync(id, request.Title, request.Description ?? string.Empty,
            request.Price, request.Genre ?? string.Empty, request.ReleaseYear, ct);
        return Ok(new GameResponse(game.Id, game.Title, game.Description, game.Price, game.Genre, game.ReleaseYear, game.CreatedAt, game.PromotionalPrice));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _gameService.DeleteAsync(id, ct);
        return NoContent();
    }
}
