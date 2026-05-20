using System.Text.RegularExpressions;

namespace FCG.Monolith.Domain.ValueObjects;

public static class Password
{
    // Min 8 chars, one uppercase, one lowercase, one digit, one special char
    private static readonly Regex PasswordRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
        RegexOptions.Compiled);

    public static void Validate(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        if (!PasswordRegex.IsMatch(password))
            throw new ArgumentException(
                "Password must be at least 8 characters and contain uppercase, lowercase, digit, and special character.",
                nameof(password));
    }
}
