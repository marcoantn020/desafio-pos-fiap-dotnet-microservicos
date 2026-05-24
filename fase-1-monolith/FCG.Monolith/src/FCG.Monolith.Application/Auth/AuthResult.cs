namespace FCG.Monolith.Application.Auth;

public record AuthResult(string Token, Guid UserId, string Name, string Email, string Role);
