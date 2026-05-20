namespace FCG.Monolith.API.DTOs.Auth;

public record AuthResponse(
    string Token,
    string UserId,
    string Name,
    string Email,
    string Role
);
