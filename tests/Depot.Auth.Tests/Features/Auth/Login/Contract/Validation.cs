namespace Depot.Auth.Tests.Features.Auth.Login.Contract;

public class Login_Validation
{
    private static readonly Faker Faker = new();

    private static readonly string Password = Faker.Internet.StrongPassword();

    private static readonly string Username = Faker.Internet.UserName();

    private static readonly string Email = Faker.Internet.Email();

    public static IEnumerable<(string?, string?, string?)> Data()
    {
        yield return (null, Email, null);
        yield return (null, Email, "");
        yield return (null, Email, " ");

        yield return (null, "not-an-email", null);
        yield return (null, "not-an-email", Username);

        yield return (Username, null, null);
        yield return (Username, null, "");
        yield return (Username, null, " ");

        yield return ("", null, Username);
        yield return (" ", null, Username);
        yield return (null, "", Username);
        yield return (null, " ", Username);
        yield return (null, null, Username);

        yield return ("", null, "");
        yield return (" ", null, "");
        yield return (" ", null, " ");
        yield return (null, "", null);
        yield return (null, " ", "");
        yield return (null, " ", " ");
        yield return ("", "", "");
        yield return (" ", "", "");
        yield return ("", " ", "");
        yield return (" ", " ", "");

        yield return (Username, Email, Password);
        yield return (Email, null, Password);
        yield return (null, Username, Password);
        yield return (Email, null, Password);

        yield return (Username, "", Password);
        yield return ("", Email, Password);

        yield return (Username, " ", Password);
        yield return (" ", Email, Password);

        yield return (Username, Email, "");
        yield return (Username, Email, " ");

        yield return (null, null, null);
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
    public async Task Login_WithInvalidPayload_ReturnsBadRequest(string? username, string? email, string? password)
    {
        var payload = new LoginCommand
        {
            Username = username, Email = email, Password = password!
        };

        var response = await Requests.Login(payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        var result = await response.ReadAsAsync<ProblemDetails>();

        var content = await Assert.That(result).IsNotNull();

        await Assert.That(content.Title).IsEqualTo(ReasonPhrases.GetReasonPhrase(400));
        await Assert.That(content.Status).IsEqualTo(400);
        await Assert.That(content.Detail!).IsNotEmpty();

        await Assert.That(content.Extensions["errors"]).IsNotNull();
    }
}