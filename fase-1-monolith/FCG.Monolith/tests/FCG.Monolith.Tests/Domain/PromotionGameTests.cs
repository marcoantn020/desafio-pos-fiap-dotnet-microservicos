using FCG.Monolith.Domain.Entities;
using FluentAssertions;

namespace FCG.Monolith.Tests.Domain;

public class PromotionGameTests
{
    [Fact]
    public void Create_ValidGuids_ReturnsCorrectValues()
    {
        var promotionId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var pg = PromotionGame.Create(promotionId, gameId);

        pg.PromotionId.Should().Be(promotionId);
        pg.GameId.Should().Be(gameId);
        pg.AddedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_EmptyPromotionId_ThrowsArgumentException()
    {
        var act = () => PromotionGame.Create(Guid.Empty, Guid.NewGuid());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_EmptyGameId_ThrowsArgumentException()
    {
        var act = () => PromotionGame.Create(Guid.NewGuid(), Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }
}
