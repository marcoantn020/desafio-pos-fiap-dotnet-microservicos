using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Enums;
using FluentAssertions;

namespace FCG.Monolith.Tests.Domain;

public class UserEntityTests
{
    [Fact]
    public void Create_ValidInputs_ReturnsUserWithCorrectValues()
    {
        var user = User.Create("Alice", "alice@example.com", "hashedpw");

        user.Name.Should().Be("Alice");
        user.Email.Should().Be("alice@example.com");
        user.PasswordHash.Should().Be("hashedpw");
        user.Role.Should().Be(UserRole.User);
        user.Id.Should().NotBeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithAdminRole_SetsAdminAndIsAdminReturnsTrue()
    {
        var user = User.Create("Admin", "admin@example.com", "hashedpw", UserRole.Admin);

        user.Role.Should().Be(UserRole.Admin);
        user.IsAdmin().Should().BeTrue();
    }

    [Fact]
    public void Create_WithDefaultRole_IsAdminReturnsFalse()
    {
        var user = User.Create("User", "user@example.com", "hashedpw");
        user.IsAdmin().Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_ThrowsArgumentException(string name)
    {
        var act = () => User.Create(name, "user@example.com", "hashedpw");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_EmptyPasswordHash_ThrowsArgumentException()
    {
        var act = () => User.Create("Name", "user@example.com", "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateName_ValidName_UpdatesName()
    {
        var user = User.Create("Old Name", "user@example.com", "hashedpw");
        user.UpdateName("New Name");
        user.Name.Should().Be("New Name");
    }

    [Fact]
    public void UpdateName_EmptyName_ThrowsArgumentException()
    {
        var user = User.Create("Name", "user@example.com", "hashedpw");
        var act = () => user.UpdateName("");
        act.Should().Throw<ArgumentException>();
    }
}
