using FCG.Monolith.Domain.Entities;
using FluentAssertions;

namespace FCG.Monolith.Tests.Domain;

public class GameEntityTests
{
    [Fact]
    public void Create_ValidInputs_ReturnsGameWithCorrectValues()
    {
        var game = Game.Create("Test Game", "A great game", 29.99m, "Action", 2024);

        game.Title.Should().Be("Test Game");
        game.Description.Should().Be("A great game");
        game.Price.Should().Be(29.99m);
        game.Genre.Should().Be("Action");
        game.ReleaseYear.Should().Be(2024);
        game.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyTitle_ThrowsArgumentException(string title)
    {
        var act = () => Game.Create(title, "desc", 9.99m, "RPG", 2024);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_NegativePrice_ThrowsArgumentException()
    {
        var act = () => Game.Create("Game", "desc", -1m, "RPG", 2024);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ZeroPrice_Succeeds()
    {
        var act = () => Game.Create("Free Game", "desc", 0m, "RPG", 2024);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(1960)]
    [InlineData(1969)]
    public void Create_InvalidReleaseYear_ThrowsArgumentException(int year)
    {
        var act = () => Game.Create("Game", "desc", 9.99m, "RPG", year);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_ValidInputs_UpdatesAllFields()
    {
        var game = Game.Create("Old Title", "Old desc", 9.99m, "RPG", 2020);
        game.Update("New Title", "New desc", 19.99m, "Action", 2024);

        game.Title.Should().Be("New Title");
        game.Description.Should().Be("New desc");
        game.Price.Should().Be(19.99m);
        game.Genre.Should().Be("Action");
        game.ReleaseYear.Should().Be(2024);
    }

    [Fact]
    public void Update_EmptyTitle_ThrowsArgumentException()
    {
        var game = Game.Create("Title", "desc", 9.99m, "RPG", 2024);
        var act = () => game.Update("", "desc", 9.99m, "RPG", 2024);
        act.Should().Throw<ArgumentException>();
    }
}
