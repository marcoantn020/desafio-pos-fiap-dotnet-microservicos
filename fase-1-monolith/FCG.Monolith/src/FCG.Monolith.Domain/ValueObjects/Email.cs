using System.Text.RegularExpressions;

namespace FCG.Monolith.Domain.ValueObjects;

public sealed class Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty.", nameof(value));

        var normalized = value.Trim().ToLowerInvariant();
        if (!EmailRegex.IsMatch(normalized))
            throw new ArgumentException($"'{value}' is not a valid email address.", nameof(value));

        return new Email(normalized);
    }

    public override string ToString() => Value;
    public static implicit operator string(Email email) => email.Value;
}
