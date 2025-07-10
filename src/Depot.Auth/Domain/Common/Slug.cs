namespace Depot.Auth.Domain.Common;

using System.Text.RegularExpressions;

public sealed record Slug : IComparable<Slug>
{
    private Slug(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public int CompareTo(Slug? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (other is null)
        {
            return 1;
        }

        return string.Compare(Value, other.Value, StringComparison.Ordinal);
    }

    public static Slug Create(string value)
    {
        return new Slug(Regex.Replace(value.ToLowerInvariant().Trim(), "[^a-z0-9]+", "-").Trim('-'));
    }

    public static implicit operator Slug(string slug)
    {
        return Create(slug);
    }

    public static implicit operator string(Slug slug)
    {
        return slug.Value;
    }
}