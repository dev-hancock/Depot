namespace Depot.Auth.Tests.Features.Auth.Login.Contract;

public class Login_Success
{
    private static readonly Faker Faker = new();

    private static readonly string Password = Faker.Internet.StrongPassword();

    private static readonly string Username = Faker.Internet.UserName();

    private static readonly string Email = Faker.Internet.Email();

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
    public async Task Login_WithValidPayload_ReturnsSession(string? username, string? email)
    {
        var payload = new LoginCommand
        {
            Username = username, Email = email, Password = Password
        };

        var response = await Requests.Login(payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.ReadAsAsync<LoginResponse>();

        var session = await Assert.That(result).IsNotNull();

        await Assert.That(session.AccessToken).IsNotEmpty();
        await Assert.That(session.RefreshToken).IsNotEmpty();
    }
}