using FCG.Monolith.API.DTOs.Auth;
using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;
using FCG.Monolith.Domain.ValueObjects;
using FCG.Monolith.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Monolith.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthController(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var email = Email.Create(request.Email);
        Password.Validate(request.Password);

        var existing = await _userRepository.GetByEmailAsync(email.Value, ct);
        if (existing is not null)
            return Conflict(new { error = "Email already registered." });

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.Name, email.Value, passwordHash);

        await _userRepository.AddAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);

        var token = _tokenService.GenerateToken(user);
        return Created($"/api/users/{user.Id}",
            new AuthResponse(token, user.Id.ToString(), user.Name, user.Email, user.Role.ToString()));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { error = "Invalid email or password." });

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Id.ToString(), user.Name, user.Email, user.Role.ToString()));
    }
}
