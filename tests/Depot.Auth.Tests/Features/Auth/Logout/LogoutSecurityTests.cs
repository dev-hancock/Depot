namespace Depot.Auth.Tests.Features.Auth.Logout;

using System.Net;
using Depot.Auth.Features.Auth.Logout;
using Microsoft.IdentityModel.Tokens;

[ClassDataSource(typeof(IntegrationFixture))]
public class LogoutSecurityTests : IntegrationTest
{
    [Test]
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

        var response = await Fixture.Client.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
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

        var response = await Fixture.Client.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Logout_WithoutAccessToken_ShouldReturnUnauthorized()
    {
        var user = Fixture.Arrange.User.WithSession().Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var response = await Fixture.Client.Post("api/v1/auth/logout", payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Logout_WithInvalidRefreshToken_ShouldReturnNotFound()
    {
        var user = Fixture.Arrange.User.WithSession().Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = Base64UrlEncoder.Encode(Fixture.Faker.Random.Bytes(32))
        };

        var response = await Fixture.Client.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }
}