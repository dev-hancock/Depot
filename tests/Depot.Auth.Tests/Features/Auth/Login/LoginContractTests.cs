namespace Depot.Auth.Tests.Features.Auth.Login;

using System.Net;
using System.Net.Http.Json;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Login;

public class LoginContractTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Login_WithExactEmail_ShouldReturnSession()
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
    public async Task Login_WithOnlyEmail_ShouldReturnBadRequest()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Email = user.Email
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }

    [Fact]
    public async Task Login_WithUpperCaseEmail_ShouldReturnSession()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Email = user.Email.ToUpperInvariant(),
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
    }

    [Fact]
    public async Task Login_WithLowerCaseEmail_ShouldReturnSession()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Email = user.Email.ToLowerInvariant(),
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
    }

    [Fact]
    public async Task Login_WithPaddedEmail_ShouldReturnSession()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Email = $" {user.Email} ",
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
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

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }


    [Fact]
    public async Task Login_WithExactPassword_ShouldReturnSession()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Email = user.Email,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
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

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }

    [Fact]
    public async Task Login_WithPaddedPassword_ShouldReturnBadRequest()
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

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }

    [Theory]
    [InlineData("")]
    [InlineData("password")]
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

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }

    [Fact]
    public async Task Login_WithEmptyPayload_ShouldReturnBadRequest()
    {
        var payload = new LoginCommand();

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }


    [Fact]
    public async Task Login_WithExactUsername_ShouldReturnSession()
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

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }

    [Fact]
    public async Task Login_WithUpperCaseUsername_ShouldReturnSession()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = user.Username.ToUpperInvariant(),
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
    }

    [Fact]
    public async Task Login_WithLowerCaseUsername_ShouldReturnSession()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = user.Username.ToLowerInvariant(),
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
    }

    [Fact]
    public async Task Login_WithPaddedUsername_ShouldReturnSession()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = $" {user.Username} ",
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
    }

    [Theory]
    [InlineData("not-an-email")]
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

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }
}