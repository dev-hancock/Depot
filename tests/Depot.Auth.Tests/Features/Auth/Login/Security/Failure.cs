namespace Depot.Auth.Tests.Features.Auth.Login.Security;

public class Login_Failure
{
    private static readonly Faker Faker = new();

    private static readonly string Password = Faker.Internet.StrongPassword();

    private static readonly string Username = Faker.Internet.UserName();

    private static readonly string Email = Faker.Internet.Email();

    public static IEnumerable<(string?, string?, string?)> Data()
    {
        yield return (Username, null, Faker.Internet.StrongPassword());
        yield return (null, Email, Faker.Internet.StrongPassword());

        yield return (Faker.Internet.UserName(), null, Password);
        yield return (null, Faker.Internet.Email(), Password);

        yield return (Username, null, $" {Faker.Internet.StrongPassword()} ");
        yield return (null, Email, $" {Faker.Internet.StrongPassword()} ");
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
    public async Task Login_WithWrongCredentials_ReturnsUnauthorized(string? username, string? email, string? password)
    {
        var payload = new LoginCommand
        {
            Username = username, Email = email, Password = password!
        };

        var response = await Requests.Login(payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);

        var result = await response.ReadAsAsync<ProblemDetails>();

        _ = await Assert.That(result).IsNotNull();
    }
}