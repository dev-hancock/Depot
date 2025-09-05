using Depot.Auth.Features.Auth.Refresh;

namespace Depot.Auth.Tests.Features.Auth.Refresh;

public class RefreshSuccessTests : TestBase
{
    private static readonly JwtSecurityTokenHandler Handler = new();

    private static readonly Guid UserId = Guid.NewGuid();

    private static readonly Guid SessionId = Guid.NewGuid();

    private const string RefreshToken = "valid-refresh-token";

    private static async Task AssertSession(RefreshResponse content)
    {
        await Assert.That(content.AccessToken).IsNotEmpty();
        await Assert.That(content.RefreshToken).IsNotEmpty();
        await Assert.That(content.RefreshToken).IsNotEqualTo(RefreshToken);
    }

    private static async Task AssertToken(JwtSecurityToken token)
    {
        await Assert.That(token.GetUser()).IsEqualTo(UserId);
        await Assert.That(token.GetSession()).IsEqualTo(SessionId);
    }

    private async Task AssertDatabase(JwtSecurityToken token)
    {
        var id = token.GetSession();

        var entity = await Fixture.Db.FindSessionAsync(id);

        var session = await Assert.That(entity).IsNotNull();

        await Assert.That(session).IsNotNull();

        // TODO: more assertions
    }

    private async Task AssertCache(JwtSecurityToken token)
    {
        var id = token.GetSession();

        var exists = await Fixture.Cache.GetSessionAsync(id);

        await Assert.That(exists).IsEqualTo(2);
    }

    [Test]
    public async Task Refresh_WithValidRefreshToken_ReturnsAndUpdatesSession()
    {
        var user = Arrange.User
            .WithId(UserId)
            .WithSession(x => x
                .WithId(SessionId)
                .WithRefreshToken(RefreshToken)
                .WithExpiry(DateTime.UtcNow.AddHours(1)))
            .Build();

        await Fixture.SeedAsync(user);

        var payload = new RefreshCommand
        {
            RefreshToken = RefreshToken
        };

        var response = await Api.Refresh(payload)
            .Authorize(x => x.WithUser(user))
            .SendAsync();

        await Assert.That(response.StatusCode).IsOk();

        var result = await response.ReadAsAsync<RefreshResponse>();

        var content = await Assert.That(result).IsNotNull();

        await AssertSession(content);

        var token = Handler.ReadJwtToken(content.AccessToken);

        await AssertToken(token);

        await AssertDatabase(token);

        await AssertCache(token);
    }
}