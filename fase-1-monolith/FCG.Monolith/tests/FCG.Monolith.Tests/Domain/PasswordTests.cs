using FCG.Monolith.Domain.ValueObjects;
using FluentAssertions;

namespace FCG.Monolith.Tests.Domain;

public class PasswordTests
{
    [Theory]
    [InlineData("Valid@123")]
    [InlineData("Abc!1234")]
    [InlineData("Str0ng#Password")]
    public void Validate_ValidPassword_DoesNotThrow(string password)
    {
        var act = () => Password.Validate(password);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("short1!")]
    [InlineData("alllowercase1!")]
    [InlineData("ALLUPPERCASE1!")]
    [InlineData("NoSpecialChar1")]
    [InlineData("NoNumber@Abc")]
    public void Validate_InvalidPassword_ThrowsArgumentException(string password)
    {
        var act = () => Password.Validate(password);
        act.Should().Throw<ArgumentException>();
    }
}
