using Microsoft.IdentityModel.Tokens;

namespace Depot.Auth.Tests.Features.Auth.Logout.Security;

public class Logout_Security
{
    private static readonly Faker Faker = new();

    [Test]
    public async Task Logout_WithExpiredRefreshToken_ReturnsNoContent()
    {
        var user = Arrange.User
            .WithSession(x =>
                x.WithExpiry(DateTime.UtcNow.AddDays(-1)))
            .Build();

        await Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var response = await Requests.Logout(payload)
            .WithAuthorization(x => x.WithUser(user))
            .SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Logout_WithInvalidRefreshToken_ReturnsNoContent()
    {
        var user = Arrange.User
            .WithSession(x => x.WithRevoked())
            .Build();

        await Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = Base64UrlEncoder.Encode(Faker.Random.Bytes(32))
        };

        var response = await Requests.Logout(payload)
            .WithAuthorization(x => x.WithUser(user))
            .SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }


    [Test]
    public async Task Logout_WithRevokedRefreshToken_ReturnsNoContent()
    {
        var user = Arrange.User
            .WithSession(x => x.WithRevoked())
            .Build();

        await Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var response = await Requests.Logout(payload)
            .WithAuthorization(x => x.WithUser(user))
            .SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }
}