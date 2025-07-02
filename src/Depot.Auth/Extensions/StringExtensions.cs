namespace Depot.Auth.Extensions;

public static class StringExtensions
{
    public static string Capitalize(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(input));
        }

        var words = input.Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return string.Join(" ", words.Select(CapitalizeWord));
    }

    private static string CapitalizeWord(string word)
    {
        return IsAllUpper(word)
            ? word
            : char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();
    }

    private static bool IsAllUpper(string word)
    {
        return word.All(char.IsUpper);
    }
}