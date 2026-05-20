namespace FCG.Monolith.Domain.Entities;

public class Game
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Genre { get; private set; } = string.Empty;
    public int ReleaseYear { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Game() { }

    public static Game Create(string title, string description, decimal price, string genre, int releaseYear)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        if (price < 0) throw new ArgumentException("Price cannot be negative.", nameof(price));
        if (releaseYear < 1970 || releaseYear > DateTime.UtcNow.Year + 2)
            throw new ArgumentException("Invalid release year.", nameof(releaseYear));

        return new Game
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Price = price,
            Genre = genre?.Trim() ?? string.Empty,
            ReleaseYear = releaseYear,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string description, decimal price, string genre, int releaseYear)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        if (price < 0) throw new ArgumentException("Price cannot be negative.", nameof(price));
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        Genre = genre?.Trim() ?? string.Empty;
        ReleaseYear = releaseYear;
    }
}
