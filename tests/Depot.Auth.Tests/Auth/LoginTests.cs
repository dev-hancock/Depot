namespace Depot.Auth.Tests.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Bogus;
using Data;
using Factories;
using Features.Auth.Login;
using Fixtures;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

public class LoginTests : IClassFixture<InfraFixture>, IAsyncLifetime
{
    private static readonly Faker Faker = new();

    private readonly InfraFixture _fixture;

    private readonly JwtSecurityTokenHandler _handler = new();

    private IDistributedCache _cache = null!;

    private HttpClient _client = null!;

    private TestUser _user = null!;

    public LoginTests(InfraFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var factory = new AuthAppFactory(_fixture);

        _client = factory.CreateClient();

        _cache = factory.Services.GetRequiredService<IDistributedCache>();

        _user = await SeedData.Users.SeedAsync(factory.Services);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Login_WithEmptyPayload_ShouldReturnBadRequest()
    {
        // Arrange
        var payload = new LoginCommand();

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Login_WithInvalidUsername_ShouldReturnBadRequest(string? username)
    {
        // Arrange
        var payload = new LoginCommand
        {
            Username = username,
            Password = _user.Password
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Login_WithInvalidEmail_ShouldReturnBadRequest(string? email)
    {
        // Arrange
        var payload = new LoginCommand
        {
            Email = email,
            Password = _user.Password
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Login_WithInvalidPassword_ShouldReturnBadRequest(string? password)
    {
        // Arrange
        var payload = new LoginCommand
        {
            Username = _user.Username,
            Email = _user.Email,
            Password = password!
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task Login_WithOnlyPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var payload = new LoginCommand
        {
            Password = Faker.Internet.Password()
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task Login_WithOnlyUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var payload = new LoginCommand
        {
            Username = _user.Username
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task Login_WithOnlyEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var payload = new LoginCommand
        {
            Email = _user.Email
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var payload = new LoginCommand
        {
            Username = _user.Username,
            Password = Faker.Internet.Password()
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ShouldReturnUnauthorized()
    {
        // Arrange
        var payload = new LoginCommand
        {
            Username = Faker.Internet.UserName(),
            Password = _user.Password
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    // [Theory]
    // [MemberData(nameof(TestData.Usernames), MemberType = typeof(TestData))]
    public async Task Login_WithValidUsername_ShouldReturnSession(string username)
    {
        // Arrange
        var payload = new LoginCommand
        {
            Username = username,
            Password = _user.Password
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        await AssertSession(result);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ShouldReturnUnauthorized()
    {
        // Arrange
        var payload = new LoginCommand
        {
            Email = Faker.Internet.Email(),
            Password = _user.Password
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    // [Theory]
    // [MemberData(nameof(TestData.Emails), MemberType = typeof(TestData))]
    public async Task Login_WithValidEmail_ShouldReturnSession(string email)
    {
        // Arrange
        var payload = new LoginCommand
        {
            Email = email,
            Password = _user.Password
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        await AssertSession(result);
    }

    [Fact]
    public async Task Login_WithValidEmailAndUsername_ShouldReturnSession()
    {
        // Arrange
        var payload = new LoginCommand
        {
            Email = _user.Email,
            Username = _user.Username,
            Password = _user.Password
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        await AssertSession(result);
    }

    [Fact]
    public async Task Login_WithPaddedPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var payload = new LoginCommand
        {
            Email = _user.Email,
            Password = $" {_user.Password} "
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    private async Task AssertSession(HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(result);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);

        var token = _handler.ReadJwtToken(result.AccessToken);

        var sessionId = token.Claims.SingleOrDefault(x => x.Type == "jti");

        Assert.NotNull(sessionId);

        var exists = await _cache.GetAsync(sessionId.Value);

        Assert.NotNull(exists);
        Assert.Equal([0x1], exists);
    }
}