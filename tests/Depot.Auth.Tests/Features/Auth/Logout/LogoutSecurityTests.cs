namespace Depot.Auth.Tests.Features.Auth.Logout;

using System.Net;
using Depot.Auth.Features.Auth.Logout;
using Login;
using Microsoft.IdentityModel.Tokens;

public class LogoutSecurityTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Logout_WithoutRevokedRefreshToken_ShouldReturnNotFound()
    {
        var user = Fixture.Arrange.User
            .WithSession(x => x.WithRevoked())
            .Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var result = await Fixture.Client.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutExpiredRefreshToken_ShouldReturnBadRequest()
    {
        var user = Fixture.Arrange.User
            .WithSession(x =>
                x.WithExpiry(DateTime.UtcNow.AddDays(-1)))
            .Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var result = await Fixture.Client.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutAccessToken_ShouldReturnUnauthorized()
    {
        var user = Fixture.Arrange.User.WithSession().Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var result = await Fixture.Client.Post("api/v1/auth/logout", payload).SendAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task Logout_WithInvalidRefreshToken_ShouldReturnNotFound()
    {
        var user = Fixture.Arrange.User.WithSession().Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = Base64UrlEncoder.Encode(Fixture.Faker.Random.Bytes(32))
        };

        var result = await Fixture.Client.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }
}