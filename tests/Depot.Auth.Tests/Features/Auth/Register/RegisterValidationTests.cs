namespace Depot.Auth.Tests.Features.Auth.Register;

public class RegisterValidationTests : TestBase
{
    private static readonly Faker Faker = new();

    private static readonly string Username = Faker.Internet.UserName();

    private static readonly string Email = Faker.Internet.Email();

    public static IEnumerable<string> Usernames()
    {
        foreach (var variant in Variants(Username))
        {
            yield return variant;
        }
    }

    public static IEnumerable<string> Emails()
    {
        foreach (var variant in Variants(Email))
        {
            yield return variant;
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

    private static async Task<ProblemDetails> AssertResponse(HttpResponseMessage response, HttpStatusCode code)
    {
        await Assert.That(response.StatusCode).IsEqualTo(code);

        var result = await response.ReadAsAsync<ProblemDetails>();

        return await Assert.That(result).IsNotNull();
    }

    private static async Task AssertProblem(ProblemDetails content, int code)
    {
        await Assert.That(content.Title).IsEqualTo(ReasonPhrases.GetReasonPhrase(code));
        await Assert.That(content.Status).IsEqualTo(code);
        await Assert.That(content.Detail!).IsNotEmpty();
    }

    private static async Task AssertErrors(ProblemDetails content, params string[] expected)
    {
        await Assert.That(content.Extensions["errors"]).IsNotNull();

        // TODO: Assert error codes
    }

    [Before(Class)]
    public static async Task Setup()
    {
        var instance = await TestFixture.Instance;

        var user = Arrange.User
            .WithUsername(Username)
            .WithEmail(Email)
            .Build();

        await instance.SeedAsync(user);
    }

    [Test]
    public async Task Register_WithEmptyPayload_ReturnsBadRequest()
    {
        var payload = new RegisterCommand();

        var response = await Api.Register(payload).SendAsync();

        var content = await AssertResponse(response, HttpStatusCode.BadRequest);

        await AssertProblem(content, (int)HttpStatusCode.BadRequest);

        await AssertErrors(content);
    }

    [Test]
    [MethodDataSource(nameof(Emails))]
    public async Task Register_WithExistingEmail_ReturnsConflict(string email)
    {
        var payload = new RegisterCommand
        {
            Username = Faker.Internet.UserName(),
            Email = email,
            Password = Faker.Internet.StrongPassword()
        };

        var response = await Api.Register(payload).SendAsync();

        var content = await AssertResponse(response, HttpStatusCode.Conflict);

        await AssertProblem(content, (int)HttpStatusCode.Conflict);
    }

    [Test]
    [MethodDataSource(nameof(Usernames))]
    public async Task Register_WithExistingUsername_ReturnsConflict(string username)
    {
        var payload = new RegisterCommand
        {
            Username = username,
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.StrongPassword()
        };

        var response = await Api.Register(payload).SendAsync();

        var content = await AssertResponse(response, HttpStatusCode.Conflict);

        await AssertProblem(content, (int)HttpStatusCode.Conflict);
    }

    [Test]
    [Arguments("not-an-email")]
    [Arguments("")]
    [Arguments(null)]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest(string? email)
    {
        var payload = new RegisterCommand
        {
            Username = Faker.Internet.UserName(),
            Email = email!,
            Password = Faker.Internet.StrongPassword()
        };

        var response = await Api.Register(payload).SendAsync();

        var content = await AssertResponse(response, HttpStatusCode.BadRequest);

        await AssertProblem(content, (int)HttpStatusCode.BadRequest);

        await AssertErrors(content);
    }

    [Test]
    [Arguments("password")]
    [Arguments("")]
    [Arguments(null)]
    public async Task Register_WithInvalidPassword_ReturnsBadRequest(string? password)
    {
        var payload = new RegisterCommand
        {
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            Password = password!
        };

        var response = await Api.Register(payload).SendAsync();

        var content = await AssertResponse(response, HttpStatusCode.BadRequest);

        await AssertProblem(content, (int)HttpStatusCode.BadRequest);

        await AssertErrors(content);
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    public async Task Register_WithInvalidUsername_ReturnsBadRequest(string? username)
    {
        var payload = new RegisterCommand
        {
            Username = username!,
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.StrongPassword()
        };

        var response = await Api.Register(payload).SendAsync();

        var content = await AssertResponse(response, HttpStatusCode.BadRequest);

        await AssertProblem(content, (int)HttpStatusCode.BadRequest);

        await AssertErrors(content);
    }

    [Test]
    public async Task Register_WithoutEmailOrUsername_ReturnsBadRequest()
    {
        var payload = new RegisterCommand
        {
            Password = Faker.Internet.StrongPassword()
        };

        var response = await Api.Register(payload).SendAsync();

        var content = await AssertResponse(response, HttpStatusCode.BadRequest);

        await AssertProblem(content, (int)HttpStatusCode.BadRequest);

        await AssertErrors(content);
    }
}