namespace Depot.Auth.Tests.Features.Auth.Login;

public class LoginSuccessTests
{
    private static readonly JwtSecurityTokenHandler Handler = new();

    private static readonly Faker Faker = new();

    private static readonly Guid Id = Guid.NewGuid();

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
        await Assert.That(token.GetUserId()).IsEqualTo(Id.ToString());
    }

    private static async Task AssertDatabase(string id)
    {
        var entity = await Database.FindSessionAsync(id);

        var session = await Assert.That(entity).IsNotNull();

        await Assert.That(session).IsNotNull();

        // TODO: more assertions
    }

    private static async Task AssertCache(string id)
    {
        var exists = await Cache.GetAsync(id);

        await Assert.That(exists).IsNotNull();
    }

    [Before(Class)]
    public static async Task Setup()
    {
        var user = Arrange.User
            .WithId(Id)
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

        var id = token.GetSessionId();

        await AssertDatabase(id);

        await AssertCache(id);
    }
}