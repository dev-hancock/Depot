namespace Depot.Auth.Tests.Data;

using Bogus;
using Extensions;

public static class TestData
{
    private static readonly Faker Faker = new();

    public static string Username { get; } = Faker.Internet.UserName();

    public static string Password { get; } = Faker.Internet.StrongPassword();

    public static string Email { get; } = Faker.Internet.Email();

    public static IEnumerable<object[]> Usernames()
    {
        yield return [Username];
        yield return [$" {Username} "];
        yield return [Username.ToUpperInvariant()];
        yield return [Username.ToLowerInvariant()];
    }

    public static IEnumerable<object[]> Emails()
    {
        yield return [Email];
        yield return [$" {Email} "];
        yield return [Email.ToUpperInvariant()];
        yield return [Email.ToLowerInvariant()];
    }
}