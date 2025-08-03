namespace Depot.Auth.Tests.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Bogus;
using Data;
using Domain.Interfaces;
using Domain.Users;
using Features.Auth.Register;
using Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

public class RegisterTests : IClassFixture<InfraFixture>, IAsyncLifetime
{
    private static readonly Faker Faker = new();

    private readonly InfraFixture _fixture;

    private readonly JwtSecurityTokenHandler _handler = new();

    private IDistributedCache _cache = null!;

    private HttpClient _client = null!;

    public RegisterTests(InfraFixture fixture)
    {
        _fixture = fixture;
    }
    //
    // private static string Username { get; } = Faker.Internet.UserName();
    //
    // private static string Password { get; } = Faker.Internet.Password();
    //
    // private static string Email { get; } = Faker.Internet.Email();

    private User User { get; set; } = null!;

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

        User = User.Create(
            Username.Create(Faker.Internet.UserName()),
            Email.Create(Faker.Internet.Email()),
            Password.Create(hasher.Hash(Faker.Internet.Password())),
            DateTime.UtcNow);

        await using var context = services.GetRequiredService<AuthDbContext>();

        await context.Database.EnsureDeletedAsync();

        await context.Database.EnsureCreatedAsync();

        context.Users.Add(User);

        await context.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Theory]
    [MemberData(nameof(TestData.Usernames), MemberType = typeof(TestData))]
    public async Task Register_WithExistingUsername_ShouldReturnConflict(string username)
    {
        // Arrange
        var payload = new RegisterCommand
        {
            Username = username,
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.Password()
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
    }

    [Theory]
    [MemberData(nameof(TestData.Emails), MemberType = typeof(TestData))]
    public async Task Register_WithExistingEmail_ShouldReturnConflict(string email)
    {
        // Arrange
        var payload = new RegisterCommand
        {
            Username = Faker.Internet.UserName(),
            Email = email,
            Password = Faker.Internet.Password()
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
    }

    [Fact]
    public async Task Register_WithValidPayload_ShouldReturnSession()
    {
        // Arrange
        var payload = new RegisterCommand
        {
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.Password()
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        await AssertSession(result);
    }

    [Fact]
    public async Task Register_WithEmptyPayload_ShouldReturnBadRequest()
    {
        // Arrange
        var payload = new RegisterCommand();

        // Act
        var result = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest(string? email)
    {
        // Arrange
        var payload = new RegisterCommand
        {
            Username = Faker.Internet.UserName(),
            Email = email!,
            Password = Faker.Internet.Password()
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Register_WithInvalidUsername_ShouldReturnBadRequest(string? username)
    {
        // Arrange
        var payload = new RegisterCommand
        {
            Username = username!,
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.Password()
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task Register_WithoutEmailOrUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var payload = new RegisterCommand
        {
            Password = Faker.Internet.Password()
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Theory]
    [InlineData("password")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Register_WithInvalidPassword_ShouldReturnBadRequest(string? password)
    {
        // Arrange
        var payload = new RegisterCommand
        {
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            Password = password!
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task Register_WithOnlyEmail_ShouldReturnSuccess()
    {
        // Arrange
        var payload = new RegisterCommand
        {
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.Password()
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        await AssertSession(result);
    }

    [Fact]
    public async Task Register_WithOnlyUsername_ShouldReturnSuccess()
    {
        // Arrange
        var payload = new RegisterCommand
        {
            Username = Faker.Internet.UserName(),
            Password = Faker.Internet.Password()
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        await AssertSession(result);
    }

    private async Task AssertSession(HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();

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