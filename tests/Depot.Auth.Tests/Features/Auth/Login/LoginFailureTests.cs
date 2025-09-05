namespace Depot.Auth.Tests.Features.Auth.Login;

public class LoginFailureTests : TestBase
{
    private static readonly Faker Faker = new();

    private static readonly Guid Id = Guid.NewGuid();

    private static readonly string Password = Faker.Internet.StrongPassword();

    private static readonly string Username = Faker.Internet.UserName();

    private static readonly string Email = Faker.Internet.Email();

    private static string WrongUsername => Faker.Internet.UserName();

    private static string WrongEmail => Faker.Internet.Email();

    private static string WrongPassword => Faker.Internet.StrongPassword();

    public static IEnumerable<(string? username, string? email, string password)> Data()
    {
        yield return (Username, null, WrongPassword);
        yield return (null, Email, WrongPassword);
        yield return (WrongUsername, null, Password);
        yield return (null, WrongEmail, Password);
        yield return (WrongUsername, null, WrongPassword);
        yield return (null, WrongEmail, WrongPassword);
    }

    private static async Task AssertProblem(ProblemDetails content)
    {
        await Assert.That(content.Title).IsEqualTo(ReasonPhrases.GetReasonPhrase(401));
        await Assert.That(content.Status).IsEqualTo(401);
        await Assert.That(content.Detail!).IsNotEmpty();
    }

    [Before(Class)]
    public static async Task Setup()
    {
        var instance = await TestFixture.Instance;

        var user = Arrange.User
            .WithId(Id)
            .WithUsername(Username)
            .WithEmail(Email)
            .WithPassword(Password)
            .Build();

        await instance.SeedAsync(user);
    }

    [Test]
    [MethodDataSource(nameof(Data))]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized(string? username, string? email, string password)
    {
        var payload = new LoginCommand
        {
            Username = username,
            Email = email,
            Password = password
        };

        var response = await Api.Login(payload).SendAsync();

        await Assert.That(response.StatusCode).IsUnauthorized();

        var result = await response.ReadAsAsync<ProblemDetails>();

        var content = await Assert.That(result).IsNotNull();

        await AssertProblem(content);
    }
}