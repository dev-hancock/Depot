namespace Depot.Auth.Tests.Features.Auth.Register;

using System.Net;
using System.Net.Http.Json;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Register;
using Login;

public class RegisterContractTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    public static IEnumerable<object[]> EquivalentUsernames => Equivalents(Faker.Internet.UserName());

    public static IEnumerable<object[]> EquivalentEmails => Equivalents(Faker.Internet.Email());

    private static IEnumerable<string> Variants(string value)
    {
        yield return value;
        yield return value.ToUpperInvariant();
        yield return value.ToLowerInvariant();
        yield return $" {value} ";
        yield return $"\t{value}\t";
        yield return $"{value}\n";
    }

    private static IEnumerable<object[]> Equivalents(string value)
    {
        return Variants(value).Select(variant => new object[] { value, variant });
    }

    [Theory]
    [MemberData(nameof(EquivalentUsernames))]
    public async Task Register_WithExistingUsername_ShouldReturnConflict(string value, string variant)
    {
        await Arrange.User.WithUsername(value).SeedAsync(Services);

        var payload = new RegisterCommand
        {
            Username = variant,
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.StrongPassword()
        };

        var result = await Client.PostAsJsonAsync("/api/v1/auth/register", payload);

        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<RegisterResponse>();

        Assert.Null(session);
    }

    [Theory]
    [MemberData(nameof(EquivalentEmails))]
    public async Task Register_WithExistingEmail_ShouldReturnConflict(string value, string variant)
    {
        await Arrange.User.WithEmail(value).SeedAsync(Services);

        var payload = new RegisterCommand
        {
            Username = variant,
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.StrongPassword()
        };

        var result = await Client.PostAsJsonAsync("/api/v1/auth/register", payload);

        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<RegisterResponse>();

        Assert.Null(session);
    }

    [Fact]
    public async Task Register_WithValidPayload_ShouldReturnSession()
    {
        var payload = new RegisterCommand
        {
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.StrongPassword()
        };

        var result = await Client.PostAsJsonAsync("/api/v1/auth/register", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<RegisterResponse>();

        Assert.NotNull(session);
    }

    [Fact]
    public async Task Register_WithEmptyPayload_ShouldReturnBadRequest()
    {
        var payload = new RegisterCommand();

        var result = await Client.PostAsJsonAsync("/api/v1/auth/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<RegisterResponse>();

        Assert.Null(session);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest(string? email)
    {
        var payload = new RegisterCommand
        {
            Username = Faker.Internet.UserName(),
            Email = email!,
            Password = Faker.Internet.StrongPassword()
        };

        var result = await Client.PostAsJsonAsync("/api/v1/auth/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<RegisterResponse>();

        Assert.Null(session);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Register_WithInvalidUsername_ShouldReturnBadRequest(string? username)
    {
        var payload = new RegisterCommand
        {
            Username = username!,
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.StrongPassword()
        };

        var result = await Client.PostAsJsonAsync("/api/v1/auth/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<RegisterResponse>();

        Assert.Null(session);
    }

    [Fact]
    public async Task Register_WithoutEmailOrUsername_ShouldReturnBadRequest()
    {
        var payload = new RegisterCommand
        {
            Password = Faker.Internet.StrongPassword()
        };

        var result = await Client.PostAsJsonAsync("/api/v1/auth/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<RegisterResponse>();

        Assert.Null(session);
    }

    [Theory]
    [InlineData("password")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Register_WithInvalidPassword_ShouldReturnBadRequest(string? password)
    {
        var payload = new RegisterCommand
        {
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            Password = password!
        };

        var result = await Client.PostAsJsonAsync("/api/v1/auth/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<RegisterResponse>();

        Assert.Null(session);
    }
}