namespace Depot.Auth.Tests.Features.Auth.Logout;

public class LogoutSuccessTests : TestBase
{
    private static readonly Faker Faker = new();

    private async Task AssertDatabase(UserId id, int expected)
    {
        var entity = await Fixture.Db.FindUserAsync(id);

        var user = await Assert.That(entity).IsNotNull();

        var revoked = user.Sessions.Where(x => x.IsRevoked).ToList();

        await Assert.That(revoked.Count).IsEqualTo(expected);

        foreach (var session in user.Sessions)
        {
            await AssertSession(session);
        }
    }

    private async Task AssertSession(Session session)
    {
        var version = await Fixture.Cache.GetSessionAsync(session.Id);

        if (session.IsRevoked)
        {
            await Assert.That(session.Version).IsEqualTo(2);
            await Assert.That(version).IsEquivalentTo(2);
        }
        else
        {
            await Assert.That(session.Version).IsEqualTo(1);
            await Assert.That(version).IsEquivalentTo(1);
        }
    }

    [Test]
    public async Task Logout_WithExpiredRefreshToken_ReturnsNoContent()
    {
        var user = Arrange.User
            .WithSession(x => x.WithExpiry(DateTime.UtcNow.AddDays(-1)))
            .Build();

        await Fixture.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var response = await Api.Logout(payload)
            .Authorize(x => x.WithUser(user))
            .SendAsync();

        await Assert.That(response.StatusCode).IsNoContent();

        await AssertDatabase(user.Id, 0);
    }

    [Test]
    public async Task Logout_WithInvalidRefreshToken_ReturnsNoContent()
    {
        var user = Arrange.User.WithSession().Build();

        await Fixture.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = Faker.Random.String(32)
        };

        var response = await Api.Logout(payload)
            .Authorize(x => x.WithUser(user))
            .SendAsync();

        await Assert.That(response.StatusCode).IsNoContent();

        await AssertDatabase(user.Id, 0);
    }

    [Test]
    public async Task Logout_WithoutRefreshToken_RevokesAllSessions()
    {
        var user = Arrange.User.WithSessions(5).Build();

        await Fixture.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = null
        };

        var response = await Api.Logout(payload)
            .Authorize(x => x.WithUser(user))
            .SendAsync();

        await Assert.That(response.StatusCode).IsNoContent();

        await AssertDatabase(user.Id, 5);
    }

    [Test]
    public async Task Logout_WithRefreshToken_RevokesSession()
    {
        var user = Arrange.User.WithSessions(5).Build();

        await Fixture.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var response = await Api.Logout(payload)
            .Authorize(x => x.WithUser(user))
            .SendAsync();

        await Assert.That(response.StatusCode).IsNoContent();

        await AssertDatabase(user.Id, 1);
    }

    [Test]
    public async Task Logout_WithRevokedRefreshToken_ReturnsNoContent()
    {
        var user = Arrange.User
            .WithSession(x => x.WithRevoked())
            .Build();

        await Fixture.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var response = await Api.Logout(payload)
            .Authorize(x => x.WithUser(user))
            .SendAsync();

        await Assert.That(response.StatusCode).IsNoContent();

        await AssertDatabase(user.Id, 1);
    }
}