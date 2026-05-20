using FCG.Monolith.Domain.ValueObjects;
using FluentAssertions;

namespace FCG.Monolith.Tests.Domain;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("user.name+tag@example.co.uk")]
    public void Create_ValidEmail_ReturnsLowercaseValue(string input)
    {
        var email = Email.Create(input);
        email.Value.Should().Be(input.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    public void Create_InvalidEmail_ThrowsArgumentException(string input)
    {
        var act = () => Email.Create(input);
        act.Should().Throw<ArgumentException>();
    }
}
