using System.ComponentModel.DataAnnotations;

namespace FCG.Monolith.API.DTOs.Auth;

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password
);
