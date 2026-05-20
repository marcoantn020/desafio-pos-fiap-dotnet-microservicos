using System.ComponentModel.DataAnnotations;

namespace FCG.Monolith.API.DTOs.Games;

public record UpdateGameRequest(
    [Required] string Title,
    string? Description,
    [Required][Range(0, double.MaxValue)] decimal Price,
    string? Genre,
    [Required][Range(1970, 2100)] int ReleaseYear
);
