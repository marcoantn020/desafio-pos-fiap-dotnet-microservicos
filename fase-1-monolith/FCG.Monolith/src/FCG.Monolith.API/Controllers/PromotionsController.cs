using FCG.Monolith.API.DTOs.Promotions;
using FCG.Monolith.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Monolith.API.Controllers;

[ApiController]
[Route("api/promotions")]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionsController(IPromotionService promotionService) => _promotionService = promotionService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var promotions = await _promotionService.GetAllActiveAsync(ct);
        return Ok(promotions.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var promotion = await _promotionService.GetByIdAsync(id, ct);
        if (promotion is null) return NotFound();
        return Ok(ToResponse(promotion));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePromotionRequest request, CancellationToken ct)
    {
        var promotion = await _promotionService.CreateAsync(
            request.Title, request.Description ?? string.Empty,
            request.DiscountPercent, request.StartsAt, request.EndsAt, ct);
        return Created($"/api/promotions/{promotion.Id}", ToResponse(promotion));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromotionRequest request, CancellationToken ct)
    {
        var promotion = await _promotionService.UpdateAsync(
            id, request.Title, request.Description ?? string.Empty,
            request.DiscountPercent, request.StartsAt, request.EndsAt, ct);
        return Ok(ToResponse(promotion));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _promotionService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/games/{gameId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddGame(Guid id, Guid gameId, CancellationToken ct)
    {
        await _promotionService.AddGameAsync(id, gameId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/games/{gameId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveGame(Guid id, Guid gameId, CancellationToken ct)
    {
        await _promotionService.RemoveGameAsync(id, gameId, ct);
        return NoContent();
    }

    private static PromotionResponse ToResponse(FCG.Monolith.Application.DTOs.PromotionDto p) =>
        new(p.Id, p.Title, p.Description, p.DiscountPercent, p.StartsAt, p.EndsAt, p.CreatedAt,
            p.Games.Select(g => new PromotionGameResponse(g.GameId, g.Title, g.Price, g.PromotionalPrice)));
}
