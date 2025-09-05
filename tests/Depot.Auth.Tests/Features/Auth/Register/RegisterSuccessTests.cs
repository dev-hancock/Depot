namespace Depot.Auth.Tests.Features.Auth.Register;

public class RegisterSuccessTests : TestBase
{
    private static readonly JwtSecurityTokenHandler Handler = new();

    private static readonly Faker Faker = new();

    private static readonly string Username = Faker.Internet.UserName();

    private static readonly string Email = Faker.Internet.Email();

    private static readonly string Password = Faker.Internet.StrongPassword();

    private static async Task AssertSession(RegisterResponse content)
    {
        await Assert.That(content.AccessToken).IsNotEmpty();
        await Assert.That(content.RefreshToken).IsNotEmpty();
    }

    private static async Task AssertToken(JwtSecurityToken token)
    {
        await Assert.That(token.GetClaim("sub")).IsNotNull();
        await Assert.That(token.GetClaim("jti")).IsNotNull();
        await Assert.That(token.GetClaim("sid")).IsNotNull();
        await Assert.That(token.GetClaim("ver")).IsNotNull().And.IsEqualTo("1");
        await Assert.That(token.GetClaim("iat")).IsNotNull().And.IsGreaterThan("0");
    }

    private async Task AssertDatabase(JwtSecurityToken token)
    {
        var id = token.GetSession();

        var entity = await Fixture.Db.FindSessionAsync(id);

        var session = await Assert.That(entity).IsNotNull();

        await Assert.That(session).IsNotNull();
        await Assert.That(session.Version).IsEqualTo(1);
        await Assert.That(session.ExpiresAt).IsAfter(DateTimeOffset.UtcNow);
        await Assert.That(session.IsRevoked).IsFalse();
        await Assert.That(session.RefreshToken).IsNotNull();
    }

    private async Task AssertCache(JwtSecurityToken token)
    {
        var key = token.GetSession();

        var version = await Fixture.Cache.GetSessionAsync(key);

        await Assert.That(version).IsEqualTo(1);
        await Assert.That(version).IsEquivalentTo(1);
    }

    [Test]
    public async Task Register_WithValidPayload_ReturnsSession()
    {
        var payload = new RegisterCommand
        {
            Username = Username,
            Email = Email,
            Password = Password
        };

        var response = await Api.Register(payload).SendAsync();

        await Assert.That(response.StatusCode).IsOk();

        var result = await response.ReadAsAsync<RegisterResponse>();

        var content = await Assert.That(result).IsNotNull();

        await AssertSession(content);

        var token = Handler.ReadJwtToken(content.AccessToken);

        await AssertToken(token);

        await AssertDatabase(token);

        await AssertCache(token);
    }
}