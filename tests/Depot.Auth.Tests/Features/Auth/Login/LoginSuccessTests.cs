using Depot.Auth.Events;

namespace Depot.Auth.Tests.Features.Auth.Login;

public class LoginSuccessTests
{
    private static readonly JwtSecurityTokenHandler Handler = new();

    private static readonly Faker Faker = new();

    private static readonly Guid UserId = Guid.NewGuid();

    private static readonly string Password = Faker.Internet.StrongPassword();

    private static readonly string Username = Faker.Internet.UserName();

    private static readonly string Email = Faker.Internet.Email();

    private static IDistributedCache Cache => Service.Get<IDistributedCache>();

    public static IEnumerable<(string?, string?)> Data()
    {
        foreach (var variant in Variants(Username))
        {
            yield return (variant, null);
        }

        foreach (var variant in Variants(Email))
        {
            yield return (null, variant);
        }
    }

    private static IEnumerable<string> Variants(string value)
    {
        yield return value;
        yield return value.ToUpperInvariant();
        yield return value.ToLowerInvariant();
        yield return $" {value} ";
        yield return $"\t{value}\t";
        yield return $"{value}\n";
    }

    private static async Task AssertSession(LoginResponse content)
    {
        await Assert.That(content.AccessToken).IsNotEmpty();
        await Assert.That(content.RefreshToken).IsNotEmpty();
    }

    private static async Task AssertToken(JwtSecurityToken token)
    {
        await Assert.That(token.GetClaim("sub")).IsNotNull().And.IsEqualTo(UserId.ToString());
        await Assert.That(token.GetClaim("jti")).IsNotNull();
        await Assert.That(token.GetClaim("sid")).IsNotNull();
        await Assert.That(token.GetClaim("ver")).IsNotNull().And.IsEqualTo("1");
        await Assert.That(token.GetClaim("iat")).IsNotNull().And.IsGreaterThan("0");
    }

    private static async Task AssertDatabase(JwtSecurityToken token)
    {
        var id = token.GetSessionId();

        var entity = await Database.FindSessionAsync(id);

        var session = await Assert.That(entity).IsNotNull();

        await Assert.That(session).IsNotNull();
        await Assert.That(session.Version).IsEqualTo(1);
        await Assert.That(session.ExpiresAt).IsAfter(DateTimeOffset.UtcNow);
        await Assert.That(session.IsRevoked).IsFalse();
        await Assert.That(session.UserId.Value).IsEqualTo(UserId);
        await Assert.That(session.RefreshToken).IsNotNull();
    }

    private static async Task AssertCache(JwtSecurityToken token)
    {
        var key = CacheKeys.Session(token.GetSessionId());

        var version = await Cache.GetAsync(key);

        await Assert.That(version).IsNotNull();
        await Assert.That(version).IsEquivalentTo(BitConverter.GetBytes(1));
    }

    [Before(Class)]
    public static async Task Setup()
    {
        var user = Arrange.User
            .WithId(UserId)
            .WithUsername(Username)
            .WithEmail(Email)
            .WithPassword(Password)
            .Build();

        await Database.SeedAsync(user);
    }

    [Test]
    [MethodDataSource(nameof(Data))]
    public async Task Login_WithValidPayload_ReturnsSession(string? username, string? email)
    {
        var payload = new LoginCommand
        {
            Username = username,
            Email = email,
            Password = Password
        };

        var response = await Requests.Login(payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.ReadAsAsync<LoginResponse>();

        var content = await Assert.That(result).IsNotNull();

        await AssertSession(content);

        var token = Handler.ReadJwtToken(content.AccessToken);

        await AssertToken(token);

        await AssertDatabase(token);

        await AssertCache(token);
    }
}