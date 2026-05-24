using FCG.Monolith.Domain.Entities;
using FluentAssertions;

namespace FCG.Monolith.Tests.Domain;

public class PromotionEntityTests
{
    private static readonly DateTime Start = DateTime.UtcNow.AddDays(1);
    private static readonly DateTime End = DateTime.UtcNow.AddDays(10);

    [Fact]
    public void Create_ValidInputs_ReturnsPromotionWithCorrectValues()
    {
        var promotion = Promotion.Create("Summer Sale", "Big discounts", 20, Start, End);

        promotion.Title.Should().Be("Summer Sale");
        promotion.Description.Should().Be("Big discounts");
        promotion.DiscountPercent.Should().Be(20);
        promotion.StartsAt.Should().Be(Start);
        promotion.EndsAt.Should().Be(End);
        promotion.Id.Should().NotBeEmpty();
        promotion.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyTitle_ThrowsArgumentException(string title)
    {
        var act = () => Promotion.Create(title, "desc", 20, Start, End);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    [InlineData(-1)]
    public void Create_InvalidDiscountPercent_ThrowsArgumentException(int discount)
    {
        var act = () => Promotion.Create("Title", "desc", discount, Start, End);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Create_BoundaryDiscountPercent_Succeeds(int discount)
    {
        var act = () => Promotion.Create("Title", "desc", discount, Start, End);
        act.Should().NotThrow();
    }

    [Fact]
    public void Create_EndsAtBeforeStartsAt_ThrowsArgumentException()
    {
        var act = () => Promotion.Create("Title", "desc", 20, End, Start);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_EndsAtEqualStartsAt_ThrowsArgumentException()
    {
        var act = () => Promotion.Create("Title", "desc", 20, Start, Start);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_ValidInputs_UpdatesAllFields()
    {
        var promotion = Promotion.Create("Old Title", "Old desc", 10, Start, End);
        var newStart = DateTime.UtcNow.AddDays(2);
        var newEnd = DateTime.UtcNow.AddDays(20);

        promotion.Update("New Title", "New desc", 50, newStart, newEnd);

        promotion.Title.Should().Be("New Title");
        promotion.Description.Should().Be("New desc");
        promotion.DiscountPercent.Should().Be(50);
        promotion.StartsAt.Should().Be(newStart);
        promotion.EndsAt.Should().Be(newEnd);
    }

    [Fact]
    public void IsActive_WhenCurrentTimeIsWithinRange_ReturnsTrue()
    {
        var promotion = Promotion.Create("Sale", "desc", 20,
            DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(10));
        promotion.IsActive().Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenNotYetStarted_ReturnsFalse()
    {
        var promotion = Promotion.Create("Sale", "desc", 20,
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));
        promotion.IsActive().Should().BeFalse();
    }
}
