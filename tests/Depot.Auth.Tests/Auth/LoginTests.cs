namespace Depot.Auth.Tests.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Bogus;
using Domain.Auth;
using Domain.Interfaces;
using Domain.Users;
using Features.Auth.Login;
using Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

public class LoginTests : IClassFixture<InfraFixture>, IAsyncLifetime
{
    private static readonly Faker Faker = new();

    private readonly InfraFixture _fixture;

    private readonly JwtSecurityTokenHandler _handler = new();

    private IDistributedCache _cache = null!;

    private HttpClient _client = null!;

    public LoginTests(InfraFixture fixture)
    {
        _fixture = fixture;
    }

    private static string Username { get; } = Faker.Internet.UserName();

    private static string Password { get; } = Faker.Internet.Password();

    private static string Email { get; } = Faker.Internet.Email();

    public async Task InitializeAsync()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host =>
            {
                host.UseEnvironment("Test");

                host.UseSetting("ConnectionStrings:Auth", _fixture.Auth);
                host.UseSetting("ConnectionStrings:Cache", _fixture.Cache);
            });

        _client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();

        var services = scope.ServiceProvider;

        var hasher = services.GetRequiredService<ISecretHasher>();

        _cache = services.GetRequiredService<IDistributedCache>();

        var user = new User(
            new UserId(Faker.Random.Guid()),
            new Username(Username.ToLowerInvariant()),
            new Email(Email.ToLowerInvariant()),
            new Password(hasher.Hash(Password)),
            DateTime.UtcNow);

        await using var context = services.GetRequiredService<AuthDbContext>();

        await context.Database.EnsureDeletedAsync();

        await context.Database.EnsureCreatedAsync();

        context.Users.Add(user);

        await context.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public static IEnumerable<object[]> ExistingUsernames()
    {
        yield return [Username];
        yield return [$" {Username} "];
        yield return [Username.ToUpperInvariant()];
        yield return [Username.ToLowerInvariant()];
    }

    public static IEnumerable<object[]> ExistingEmails()
    {
        yield return [Email];
        yield return [$" {Email} "];
        yield return [Email.ToUpperInvariant()];
        yield return [Email.ToLowerInvariant()];
    }

    [Fact]
    public async Task Login_WithEmptyPayload_ShouldReturnBadRequest()
    {
        // Arrange
        var payload = new LoginCommand();

        // Act
        var result = await _client.PostAsJsonAsync("api/auth/login", payload);

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
            Password = Password
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/auth/login", payload);

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
            Password = Password
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/auth/login", payload);

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
            Username = Username,
            Email = Email,
            Password = password!
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/auth/login", payload);

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
        var result = await _client.PostAsJsonAsync("api/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task Login_WithOnlyUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var payload = new LoginCommand
        {
            Username = Username
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task Login_WithOnlyEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var payload = new LoginCommand
        {
            Email = Email
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var payload = new LoginCommand
        {
            Username = Username,
            Password = Faker.Internet.Password()
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/auth/login", payload);

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
            Password = Password
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Theory]
    [MemberData(nameof(ExistingUsernames))]
    public async Task Login_WithValidUsername_ShouldReturnSession(string username)
    {
        // Arrange
        var payload = new LoginCommand
        {
            Username = username,
            Password = Password
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/auth/login", payload);

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
            Password = Password
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Theory]
    [MemberData(nameof(ExistingEmails))]
    public async Task Login_WithValidEmail_ShouldReturnSession(string email)
    {
        // Arrange
        var payload = new LoginCommand
        {
            Email = email,
            Password = Password
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/auth/login", payload);

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
            Email = Email,
            Username = Username,
            Password = Password
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/auth/login", payload);

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
            Email = Email,
            Password = $" {Password} "
        };

        // Act
        var result = await _client.PostAsJsonAsync("api/auth/login", payload);

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

        var session = token.Claims.SingleOrDefault(x => x.Type == "jti");

        Assert.NotNull(session);

        var exists = await _cache.GetAsync(session.Value);

        Assert.NotNull(exists);
        Assert.Equal([0x1], exists);
    }
}