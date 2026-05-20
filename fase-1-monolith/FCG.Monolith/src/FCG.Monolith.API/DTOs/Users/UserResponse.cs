namespace FCG.Monolith.API.DTOs.Users;

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    string Role,
    DateTime CreatedAt
);
