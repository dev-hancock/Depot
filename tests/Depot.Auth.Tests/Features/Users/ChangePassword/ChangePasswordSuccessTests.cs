using Depot.Auth.Domain.Auth;
using Depot.Auth.Services;

namespace Depot.Auth.Tests.Features.Users.ChangePassword;

public class ChangePasswordSuccessTests : TestBase
{
    private static readonly JwtSecurityTokenHandler Handler = new();

    private static readonly Faker Faker = new();

    private static readonly Guid UserId = Guid.NewGuid();

    private static readonly string NewPassword = Faker.Internet.StrongPassword();
    
    private static readonly string OldPassword = Faker.Internet.StrongPassword();

    private static readonly ISecretHasher Hasher = Global.GetService<ISecretHasher>();

    [Test]
    public async Task ChangePassword_WithValidPayload_ReturnsSession()
    {
        var user = Arrange.User
            .WithId(UserId)
            .WithPassword(OldPassword)
            .WithSessions(5)
            .Build();

        await Fixture.SeedAsync(user);

        var payload = new ChangePasswordCommand
        {
            NewPassword = NewPassword,
            OldPassword = OldPassword
        };

        var response = await Api.ChangePassword(payload)
            .Authorize(x => x.WithUser(user))
            .SendAsync();

        await Assert.That(response.StatusCode).IsOk();

        var result = await response.ReadAsAsync<ChangePasswordResponse>();

        var token = await AssertResponse(result);

        await AssertToken(token);

        await AssertDatabase(token);
    }

    private static async Task<JwtSecurityToken> AssertResponse(ChangePasswordResponse result)
    {
        var content = await Assert.That(result).IsNotNull();

        await Assert.That(content.AccessToken).IsNotEmpty();
        await Assert.That(content.RefreshToken).IsNotEmpty();

        return Handler.ReadJwtToken(content.AccessToken);
    }

    private static async Task AssertToken(JwtSecurityToken token)
    {
        await Assert.That(token.GetClaim("sub")).IsNotNull().And.IsEqualTo(UserId.ToString());
        await Assert.That(token.GetClaim("jti")).IsNotNull();
        await Assert.That(token.GetClaim("sid")).IsNotNull();
        await Assert.That(token.GetClaim("ver")).IsNotNull().And.IsEqualTo("1");
        await Assert.That(token.GetClaim("iat")).IsNotNull().And.IsGreaterThan("0");
    }

    private async Task AssertSession(Session? entity)
    {
        var session = await Assert.That(entity).IsNotNull();

        await Assert.That(session).IsNotNull();
        await Assert.That(session.Version).IsEqualTo(1);
        await Assert.That(session.ExpiresAt).IsAfter(DateTimeOffset.UtcNow);
        await Assert.That(session.IsRevoked).IsFalse();
        await Assert.That(session.UserId.Value).IsEqualTo(UserId);
        await Assert.That(session.RefreshToken).IsNotNull();
    }

    private async Task AssertDatabase(JwtSecurityToken token)
    {
        var id = token.GetUser();

        var entity = await Fixture.Db.FindUserAsync(id);

        var user = await Assert.That(entity).IsNotNull();

        var valid = Hasher.Verify(user.Password.Encoded, NewPassword);

        await Assert.That(valid).IsTrue();

        await Assert.That(user.Sessions).HasCount(6);

        await AssertSessions(token, user.Sessions);
    }

    private async Task AssertSessions(JwtSecurityToken token, List<Session> sessions)
    {
        var id = token.GetSession();

        foreach (var session in sessions)
        {
            if (session.Id == id)
            {
                await AssertSession(session);
            }
            else
            {
                await Assert.That(session.IsRevoked).IsTrue();
            }
        }
    }
}
