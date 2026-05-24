using FCG.Monolith.API.DTOs.Auth;
using FCG.Monolith.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Monolith.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService) => _userService = userService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _userService.RegisterAsync(request.Name, request.Email, request.Password, ct);
        return Created($"/api/users/{result.UserId}",
            new AuthResponse(result.Token, result.UserId.ToString(), result.Name, result.Email, result.Role));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _userService.LoginAsync(request.Email, request.Password, ct);
        return Ok(new AuthResponse(result.Token, result.UserId.ToString(), result.Name, result.Email, result.Role));
    }
}
