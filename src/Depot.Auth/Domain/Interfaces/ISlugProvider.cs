namespace Depot.Auth.Domain.Interfaces;

using System.Text.RegularExpressions;

public interface ISlugProvider
{
    string GetSlug(string name);
}

public class SlugProvider : ISlugProvider
{
    public string GetSlug(string name)
    {
        return Regex.Replace(name.ToLowerInvariant().Trim(), @"[^a-z0-9]+", "-").Trim('-');
    }
}