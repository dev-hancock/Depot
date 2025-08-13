namespace Depot.Auth.Domain.Common;

using System.Text.RegularExpressions;
using ErrorOr;

public sealed partial record Slug
{
    private Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
        }

        Value = SlugRegex().Replace(value.ToLowerInvariant().Trim(), "-").Trim('-');
    }

    public string Value { get; }

    public static Slug Create(string value)
    {
        return new Slug(value);
    }

    public static ErrorOr<Slug> TryCreate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation();
        }

        return new Slug(value);
    }

    public static implicit operator string(Slug slug)
    {
        return slug.Value;
    }

    public override string ToString()
    {
        return Value;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex SlugRegex();
}