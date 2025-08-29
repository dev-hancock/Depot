using System.Security.Cryptography;

namespace Depot.Auth.Tests.Setup;

public static class Unique
{
    public static string Email(string id)
    {
        return $"user_{id}@example.com";
    }

    public static string Id()
    {
        return RandomNumberGenerator.GetHexString(8, true);
    }

    public static string Username(string id)
    {
        return $"user_{id}";
    }
}