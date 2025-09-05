namespace Depot.Auth.Tests.Features.Auth.Register;

public class RegisterValidationTests : TestBase
{
    private static readonly Faker Faker = new();

    public static IEnumerable<object[]> EquivalentUsernames => Equivalents(Faker.Internet.UserName());

    public static IEnumerable<object[]> EquivalentEmails => Equivalents(Faker.Internet.Email());

    private static IEnumerable<object[]> Equivalents(string value)
    {
        return Variants(value).Select(variant => new object[]
        {
            value,
            variant
        });
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

    private static async Task AssertResponse(HttpResponseMessage response)
    {
        await Assert.That(response.StatusCode).IsBadRequest();

        var result = await response.ReadAsAsync<ProblemDetails>();

        var content = await Assert.That(result).IsNotNull();

        await AssertProblem(content);
    }

    private static async Task AssertProblem(ProblemDetails content)
    {
        await Assert.That(content.Title).IsEqualTo(ReasonPhrases.GetReasonPhrase(400));
        await Assert.That(content.Status).IsEqualTo(400);
        await Assert.That(content.Detail!).IsNotEmpty();
        await Assert.That(content.Extensions["errors"]).IsNotNull();
    }

    [Test]
    public async Task Register_WithEmptyPayload_ShouldReturnBadRequest()
    {
        var payload = new RegisterCommand();

        var response = await Api.Register(payload).SendAsync();

        await AssertResponse(response);
    }

    [Test]
    [MethodDataSource(nameof(EquivalentEmails))]
    public async Task Register_WithExistingEmail_ShouldReturnConflict(string value, string variant)
    {
        var user = Arrange.User.WithEmail(value).Build();

        await Fixture.SeedAsync(user);

        var payload = new RegisterCommand
        {
            Username = variant,
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.StrongPassword()
        };

        var response = await Api.Register(payload).SendAsync();

        await AssertResponse(response);
    }

    [Test]
    [MethodDataSource(nameof(EquivalentUsernames))]
    public async Task Register_WithExistingUsername_ShouldReturnConflict(string value, string variant)
    {
        var user = Arrange.User.WithUsername(value).Build();

        await Fixture.SeedAsync(user);

        var payload = new RegisterCommand
        {
            Username = variant,
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.StrongPassword()
        };

        var response = await Api.Register(payload).SendAsync();

        await AssertResponse(response);
    }

    [Test]
    [Arguments("not-an-email")]
    [Arguments("")]
    [Arguments(null)]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest(string? email)
    {
        var payload = new RegisterCommand
        {
            Username = Faker.Internet.UserName(),
            Email = email!,
            Password = Faker.Internet.StrongPassword()
        };

        var response = await Api.Register(payload).SendAsync();

        await AssertResponse(response);
    }

    [Test]
    [Arguments("password")]
    [Arguments("")]
    [Arguments(null)]
    public async Task Register_WithInvalidPassword_ShouldReturnBadRequest(string? password)
    {
        var payload = new RegisterCommand
        {
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            Password = password!
        };

        var response = await Api.Register(payload).SendAsync();

        await AssertResponse(response);
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    public async Task Register_WithInvalidUsername_ShouldReturnBadRequest(string? username)
    {
        var payload = new RegisterCommand
        {
            Username = username!,
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.StrongPassword()
        };

        var response = await Api.Register(payload).SendAsync();

        await AssertResponse(response);
    }

    [Test]
    public async Task Register_WithoutEmailOrUsername_ShouldReturnBadRequest()
    {
        var payload = new RegisterCommand
        {
            Password = Faker.Internet.StrongPassword()
        };

        var response = await Api.Register(payload).SendAsync();

        await AssertResponse(response);
    }

    [Test]
    public async Task Register_WithValidPayload_ShouldReturnSession()
    {
        var payload = new RegisterCommand
        {
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.StrongPassword()
        };

        var response = await Api.Register(payload).SendAsync();

        await AssertResponse(response);
    }
}