namespace Depot.Auth.Tests.Features.Auth.Login.Contract;

using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Login;
using Setup;

public class Success
{
    public static readonly string Password = "Super$ecr3t!";

    private static readonly string Hash = Unique.Hash();

    public static readonly string Username = Unique.Username(Hash);

    public static readonly string Email = Unique.Email(Hash);

    [Before(Class)]
    public async static Task Setup()
    {
        var user = Arrange.User
            .WithUsername(Username)
            .WithEmail(Email)
            .WithPassword(Password)
            .Build();

        await Database.SeedAsync(user);
    }

    private static IEnumerable<string> Variants(string value)
    {
        yield return value;
        yield return value.ToUpperInvariant();
        yield return value.ToLowerInvariant();
        yield return $" {value} ";
        yield return $"\t{value}\t";
        yield return $"{value}\n";
    }

    public static IEnumerable<(string?, string?)> Variants()
    {
        foreach (var variant in Variants(Username))
        {
            yield return (variant, null);
        }

        foreach (var variant in Variants(Email))
        {
            yield return (null, variant);
        }
    }

    [Test]
    [MethodDataSource(nameof(Variants))]
    public async Task Login_WithValidPayload_ShouldReturnSession(string? username, string? email)
    {
        var payload = new LoginCommand
        {
            Username = username,
            Email = email,
            Password = Password
        };

        var response = await Requests.Post("api/v1/auth/login", payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        var session = await Assert.That(result).IsNotNull();

        await Assert.That(session.AccessToken).IsNotEmpty();
        await Assert.That(session.RefreshToken).IsNotEmpty();
    }
}