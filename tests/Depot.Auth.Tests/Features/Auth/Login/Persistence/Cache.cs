namespace Depot.Auth.Tests.Features.Auth.Login.Persistence;

public class Login_Cache
{
    private static readonly Faker Faker = new();

    private static readonly string Password = Faker.Internet.StrongPassword();

    private static readonly string Username = Faker.Internet.UserName();

    private static readonly string Email = Faker.Internet.Email();

    public static IEnumerable<(string?, string?)> Data()
    {
        yield return (null, Email);
        yield return (Username, null);
    }

    private static IDistributedCache Cache => Service.Get<IDistributedCache>();

    [Before(Class)]
    public static async Task Setup()
    {
        var user = Arrange.User
            .WithUsername(Username)
            .WithEmail(Email)
            .WithPassword(Password)
            .Build();

        await Database.SeedAsync(user);
    }

    [Test]
    [MethodDataSource(nameof(Data))]
    public async Task Login_WithValidPayload_CachesSession(string? username, string? email)
    {
        var payload = new LoginCommand
        {
            Username = username, Email = email, Password = Password
        };

        var response = await Requests.Login(payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.ReadAsAsync<LoginResponse>();

        var content = await Assert.That(result).IsNotNull();

        var id = content.AccessToken.GetClaimValue("jti");

        var exists = await Cache.GetAsync(id);

        await Assert.That(exists).IsNotNull();
    }
}