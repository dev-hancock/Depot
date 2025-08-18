namespace Depot.Auth.Tests.Features.Auth.Login;

using System.Net;
using System.Net.Http.Json;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Login;
using Microsoft.AspNetCore.Mvc;

public class LoginContractTests(IntegrationFixture fixture) : IntegrationTest(fixture)
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
    [MemberData(nameof(EquivalentEmails))]
    public async Task Login_WithValidEmail_ShouldReturnSession(string value, string variant)
    {
        var user = await Arrange.User.WithEmail(value).SeedAsync(Services);

        var payload = new LoginCommand
        {
            Email = variant,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);

        Assert.NotNull(session.AccessToken);
        Assert.NotNull(session.RefreshToken);
    }

    [Fact]
    public async Task Login_WithOnlyEmail_ShouldReturnBadRequest()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Email = user.Email
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(content);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Login_WithInvalidEmail_ShouldReturnBadRequest(string? email)
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Email = email,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var problem = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
    }

    [Fact]
    public async Task Login_WithExactPassword_UsingEmail_ShouldReturnSession()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Email = user.Email,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);

        Assert.NotNull(session.AccessToken);
        Assert.NotNull(session.RefreshToken);
    }

    [Fact]
    public async Task Login_WithExactPassword_UsingEmailAndUsername_ShouldReturnBadRequest()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Email = user.Email,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(content);
    }

    [Fact]
    public async Task Login_WithExactPassword_UsingUsername_ShouldReturnSession()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);

        Assert.NotNull(session.AccessToken);
        Assert.NotNull(session.RefreshToken);
    }

    [Fact]
    public async Task Login_WithOnlyPassword_ShouldReturnBadRequest()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(content);
    }

    [Fact]
    public async Task Login_WithPaddedPassword_ShouldReturnUnauthorized()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Email = user.Email,
            Password = $" {user.Password} "
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(content);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Login_WithInvalidPassword_ShouldReturnBadRequest(string? password)
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Email = user.Email,
            Password = password!
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(content);
    }

    [Fact]
    public async Task Login_WithEmptyPayload_ShouldReturnBadRequest()
    {
        var payload = new LoginCommand();

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(content);
    }

    [Theory]
    [MemberData(nameof(EquivalentUsernames))]
    public async Task Login_WithValidUsername_ShouldReturnSession(string value, string variant)
    {
        var user = await Arrange.User.WithUsername(value).SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = variant,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);

        Assert.NotNull(session.AccessToken);
        Assert.NotNull(session.RefreshToken);
    }

    [Fact]
    public async Task Login_WithOnlyUsername_ShouldReturnBadRequest()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = user.Username
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(content);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Login_WithInvalidUsername_ShouldReturnBadRequest(string? username)
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = username,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(content);
    }
}