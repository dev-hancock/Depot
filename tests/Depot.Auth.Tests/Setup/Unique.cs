namespace Depot.Auth.Tests.Setup;

using System.Security.Cryptography;

public static class Unique
{
    public static string Hash()
    {
        return RandomNumberGenerator.GetHexString(8, true);
    }

    public static string Username(string hash)
    {
        return $"user_{hash}";
    }

    public static string Email(string hash)
    {
        return $"user_{hash}@example.com";
    }
}