namespace Depot.Auth.Tests.Features.Auth.Logout;

public class LogoutSuccessTests
{
    private static readonly Faker Faker = new();

    private static async Task AssertDatabase(UserId id, int expected)
    {
        var entity = await Database.FindUserAsync(id);

        var user = await Assert.That(entity).IsNotNull();

        var revoked = user.Sessions.Where(x => x.IsRevoked).ToList();

        await Assert.That(revoked.Count).IsEqualTo(expected);
    }

    [Test]
    public async Task Logout_WithExpiredRefreshToken_ReturnsNoContent()
    {
        var user = Arrange.User
            .WithSession(x => x.WithExpiry(DateTime.UtcNow.AddDays(-1)))
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

        await AssertDatabase(user.Id, 0);
    }

    [Test]
    public async Task Logout_WithInvalidRefreshToken_ReturnsNoContent()
    {
        var user = Arrange.User.WithSession().Build();

        await Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = Faker.Random.String(32)
        };

        var response = await Requests.Logout(payload)
            .WithAuthorization(x => x.WithUser(user))
            .SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        await AssertDatabase(user.Id, 0);
    }

    [Test]
    public async Task Logout_WithoutRefreshToken_RevokesAllSessions()
    {
        var user = Arrange.User.WithSessions(5).Build();

        await Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = null
        };

        var response = await Requests.Logout(payload)
            .WithAuthorization(x => x.WithUser(user))
            .SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        await AssertDatabase(user.Id, 5);
    }

    [Test]
    public async Task Logout_WithRefreshToken_RevokesSession()
    {
        var user = Arrange.User.WithSessions(5).Build();

        await Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var response = await Requests.Logout(payload)
            .WithAuthorization(x => x.WithUser(user))
            .SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        await AssertDatabase(user.Id, 1);
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

        await AssertDatabase(user.Id, 1);
    }
}