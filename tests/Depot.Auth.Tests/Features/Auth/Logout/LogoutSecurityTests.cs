namespace Depot.Auth.Tests.Features.Auth.Logout;

using System.Net;
using System.Net.Http.Json;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Logout;
using Login;
using Microsoft.IdentityModel.Tokens;

public class LogoutSecurityTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Logout_WithoutRevokedRefreshToken_ShouldReturnNotFound()
    {
        var user = await Arrange.User
            .WithSession(x =>
                x.WithRevoked(true))
            .SeedAsync(Services);

        var payload = new LogoutCommand();

        var request = CreateRequest(payload, user.Sessions[0].AccessToken);

        var result = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutExpiredRefreshToken_ShouldReturnBadRequest()
    {
        var user = await Arrange.User
            .WithSession(x =>
                x.WithExpiry(DateTime.UtcNow.AddDays(-1)))
            .SeedAsync(Services);

        var payload = new LogoutCommand();

        var request = CreateRequest(payload, user.Sessions[0].AccessToken);

        var result = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutAccessToken_ShouldReturnUnauthorized()
    {
        var user = await Arrange.User.WithSession().SeedAsync(Services);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/logout", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }


    [Fact]
    public async Task Logout_WithInvalidRefreshToken_ShouldReturnNotFound()
    {
        var user = await Arrange.User.WithSession().SeedAsync(Services);

        var payload = new LogoutCommand
        {
            RefreshToken = Base64UrlEncoder.Encode(Faker.Random.Bytes(32))
        };

        var request = CreateRequest(payload, user.Sessions[0].AccessToken);

        var result = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    private static HttpRequestMessage CreateRequest(LogoutCommand command, string token)
    {
        return new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/logout")
        {
            Content = JsonContent.Create(command),
            Headers =
            {
                { "Authorization", $"Bearer {token}" }
            }
        };
    }
}