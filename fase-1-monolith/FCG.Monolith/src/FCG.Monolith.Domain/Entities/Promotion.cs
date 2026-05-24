namespace FCG.Monolith.Domain.Entities;

public class Promotion
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int DiscountPercent { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ICollection<PromotionGame> Games { get; private set; } = new List<PromotionGame>();

    private Promotion() { }

    public static Promotion Create(string title, string description, int discountPercent, DateTime startsAt, DateTime endsAt)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (discountPercent < 1 || discountPercent > 100)
            throw new ArgumentException("DiscountPercent must be between 1 and 100.", nameof(discountPercent));
        if (endsAt <= startsAt)
            throw new ArgumentException("EndsAt must be after StartsAt.", nameof(endsAt));

        return new Promotion
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description?.Trim() ?? string.Empty,
            DiscountPercent = discountPercent,
            StartsAt = startsAt,
            EndsAt = endsAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string description, int discountPercent, DateTime startsAt, DateTime endsAt)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (discountPercent < 1 || discountPercent > 100)
            throw new ArgumentException("DiscountPercent must be between 1 and 100.", nameof(discountPercent));
        if (endsAt <= startsAt)
            throw new ArgumentException("EndsAt must be after StartsAt.", nameof(endsAt));

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        DiscountPercent = discountPercent;
        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public bool IsActive() => DateTime.UtcNow >= StartsAt && DateTime.UtcNow <= EndsAt;
}
