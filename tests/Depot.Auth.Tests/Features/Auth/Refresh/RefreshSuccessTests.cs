using Depot.Auth.Events;
using Depot.Auth.Features.Auth.Refresh;

namespace Depot.Auth.Tests.Features.Auth.Refresh;

public class RefreshSuccessTests
{
    private static readonly JwtSecurityTokenHandler Handler = new();

    private static readonly Guid UserId = Guid.NewGuid();

    private static readonly Guid SessionId = Guid.NewGuid();

    private const string RefreshToken = "valid-refresh-token";

    private static IDistributedCache Cache => Service.Get<IDistributedCache>();

    private static async Task AssertSession(RefreshResponse content)
    {
        await Assert.That(content.AccessToken).IsNotEmpty();
        await Assert.That(content.RefreshToken).IsNotEmpty();
        await Assert.That(content.RefreshToken).IsNotEqualTo(RefreshToken);
    }

    private static async Task AssertToken(JwtSecurityToken token)
    {
        await Assert.That(token.GetUserId()).IsEqualTo(UserId);
        await Assert.That(token.GetSessionId()).IsEqualTo(SessionId);
    }

    private static async Task AssertDatabase(JwtSecurityToken token)
    {
        var id = token.GetSessionId();

        var entity = await Database.FindSessionAsync(id);

        var session = await Assert.That(entity).IsNotNull();

        await Assert.That(session).IsNotNull();

        // TODO: more assertions
    }

    private static async Task AssertCache(JwtSecurityToken token)
    {
        var id = CacheKeys.Session(token.GetSessionId());

        var exists = await Cache.GetAsync(id);

        await Assert.That(exists).IsNotNull();
    }

    [Test]
    public async Task Refresh_WithValidRefreshToken_ReturnsAndUpdatesSession()
    {
        var user = Arrange.User
            .WithId(UserId)
            .WithSession(x => x
                .WithId(SessionId)
                .WithRefreshToken(RefreshToken)
                .WithExpiry(DateTime.UtcNow.AddDays(1)))
            .Build();

        await Database.SeedAsync(user);

        var payload = new RefreshCommand
        {
            RefreshToken = RefreshToken
        };

        var response = await Requests.Refresh(payload)
            .WithAuthorization(x => x.WithUser(user))
            .SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.ReadAsAsync<RefreshResponse>();

        var content = await Assert.That(result).IsNotNull();

        await AssertSession(content);

        var token = Handler.ReadJwtToken(content.AccessToken);

        await AssertToken(token);

        await AssertDatabase(token);

        await AssertCache(token);
    }
}