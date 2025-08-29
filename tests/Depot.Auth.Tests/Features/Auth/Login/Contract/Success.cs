using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Login;
using Depot.Auth.Tests.Setup;

namespace Depot.Auth.Tests.Features.Auth.Login.Contract;

public class Login_Success
{
    public static readonly string Password = "Super$ecr3t!";

    private static readonly string Id = Unique.Id();

    public static readonly string Username = Unique.Username(Id);

    public static readonly string Email = Unique.Email(Id);

    public static IEnumerable<(string?, string?)> Data()
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

    private static IEnumerable<string> Variants(string value)
    {
        yield return value;
        yield return value.ToUpperInvariant();
        yield return value.ToLowerInvariant();
        yield return $" {value} ";
        yield return $"\t{value}\t";
        yield return $"{value}\n";
    }

    [Before(Class)]
    public static async Task Setup()
    {
        var user = Arrange.User
            .WithUsername(Username)
            .WithEmail(Email)
            .WithPassword(Password)
            .Build();

        await Database.SeedAsync(user);
    }

    [Test]
    [MethodDataSource(nameof(Data))]
    public async Task Login_WithValidPayload_ReturnsSession(string? username, string? email)
    {
        var payload = new LoginCommand
        {
            Username = username, Email = email, Password = Password
        };

        var response = await Requests.Post("api/v1/auth/login", payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        var session = await Assert.That(result).IsNotNull();

        await Assert.That(session.AccessToken).IsNotEmpty();
        await Assert.That(session.RefreshToken).IsNotEmpty();
    }
}