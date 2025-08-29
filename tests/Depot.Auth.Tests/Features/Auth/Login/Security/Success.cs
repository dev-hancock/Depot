namespace Depot.Auth.Tests.Features.Auth.Login.Security;

public class Login_Success
{
    private static readonly JwtSecurityTokenHandler Handler = new();

    private static readonly Faker Faker = new();

    private static readonly Guid Id = Guid.NewGuid();

    private static readonly string Password = Faker.Internet.StrongPassword();

    private static readonly string Username = Faker.Internet.UserName();

    private static readonly string Email = Faker.Internet.Email();

    public static IEnumerable<(string?, string?)> Data()
    {
        yield return (null, Email);
        yield return (Username, null);
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
    public async Task Login_WithValidPayload_ReturnsAccessToken(string? username, string? email)
    {
        var payload = new LoginCommand
        {
            Username = username, Email = email, Password = Password
        };

        var response = await Requests.Login(payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.ReadAsAsync<LoginResponse>();

        var session = await Assert.That(result).IsNotNull();

        await Assert.That(session.AccessToken).IsNotNull();
        await Assert.That(session.RefreshToken).IsNotEmpty();

        var token = Handler.ReadJwtToken(session.AccessToken);

        await Assert.That(token.Subject).IsEqualTo(Id.ToString());
    }
}