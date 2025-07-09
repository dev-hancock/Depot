namespace Depot.Auth.Domain.Common;

using System.Text.RegularExpressions;

public class Slug
{
    private Slug(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Slug New(string value)
    {
        return new Slug(Regex.Replace(value.ToLowerInvariant().Trim(), @"[^a-z0-9]+", "-").Trim('-'));
    }
}