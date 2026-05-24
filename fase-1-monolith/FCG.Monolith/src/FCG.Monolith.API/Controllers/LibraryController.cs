using System.Security.Claims;
using FCG.Monolith.API.DTOs.Library;
using FCG.Monolith.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Monolith.API.Controllers;

[ApiController]
[Route("api/library")]
[Authorize]
public class LibraryController : ControllerBase
{
    private readonly ILibraryService _libraryService;

    public LibraryController(ILibraryService libraryService) => _libraryService = libraryService;

    [HttpGet]
    public async Task<IActionResult> GetMyLibrary(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var items = await _libraryService.GetByUserIdAsync(userId, ct);
        return Ok(items.Select(li => new LibraryItemResponse(li.GameId, li.Title, li.Genre, li.Price, li.AcquiredAt)));
    }

    [HttpPost("{gameId:guid}")]
    public async Task<IActionResult> AddToLibrary(Guid gameId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var item = await _libraryService.AddToLibraryAsync(userId, gameId, ct);
        return Created("/api/library",
            new LibraryItemResponse(item.GameId, item.Title, item.Genre, item.Price, item.AcquiredAt));
    }
}
