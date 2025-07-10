namespace Depot.Auth.Domain.Common;

using System.Text.RegularExpressions;

public sealed record Slug : IComparable<Slug>
{
    private Slug(string value)
    {
        Value = Regex.Replace(value.ToLowerInvariant().Trim(), "[^a-z0-9]+", "-").Trim('-');
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

    public static implicit operator Slug(string slug)
    {
        return new Slug(slug);
    }

    public static implicit operator string(Slug slug)
    {
        return slug.Value;
    }
}