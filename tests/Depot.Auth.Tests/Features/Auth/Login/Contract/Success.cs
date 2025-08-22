namespace Depot.Auth.Tests.Features.Auth.Login.Contract;

using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Login;

public class Success(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    private const string ValidUsername = "username";

    private const string ValidEmail = "user@example.com";

    public static IEnumerable<object?[]> Equivalents
    {
        get
        {
            foreach (var username in Variants(ValidUsername))
            {
                yield return [username, null];
            }

            foreach (var email in Variants(ValidEmail))
            {
                yield return [null, email];
            }
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

    [Theory]
    [MemberData(nameof(Equivalents))]
    public async Task Login_WithValidPayload_ShouldReturnSession(string username, string email)
    {
        var user = Fixture.Arrange.User
            .WithUsername(ValidUsername)
            .WithEmail(ValidEmail)
            .Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new LoginCommand
        {
            Username = username,
            Email = email,
            Password = user.Password
        };

        var result = await Fixture.Client.Post("api/v1/auth/login", payload).SendAsync();

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        AssertSession(session);
    }

    private static void AssertSession(LoginResponse? session)
    {
        Assert.NotNull(session);
        Assert.NotEmpty(session.AccessToken);
        Assert.NotEmpty(session.RefreshToken);
    }
}