namespace Depot.Auth.Tests.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Bogus;
using Data.Extensions;
using Data.Models;
using Factories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

public class RegisterTests : IClassFixture<InfraFixture>, IAsyncLifetime
{
    private static readonly Faker Faker = new();

    private readonly InfraFixture _fixture;

    private readonly JwtSecurityTokenHandler _handler = new();

    private IDistributedCache _cache = null!;

    private HttpClient _client = null!;

    private TestUser _user = null!;

    public RegisterTests(InfraFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var factory = new TestAppFactory(_fixture);

        _client = factory.CreateClient();

        _cache = factory.Services.GetRequiredService<IDistributedCache>();

        _user = await SeedData.Users.SeedAsync(factory.Services);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    // [Theory]
    // [MemberData(nameof(TestData.Usernames), MemberType = typeof(TestData))]
    public async Task Register_WithExistingUsername_ShouldReturnConflict(string username)
    {
        // Arrange
        var payload = new RegisterCommand
        {
            Username = username,
            Email = Faker.Internet.Email(),
            Password = Faker.Internet.StrongPassword()
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
    }

    // [Theory]
    // [MemberData(nameof(TestData.Emails), MemberType = typeof(TestData))]
    public async Task Register_WithExistingEmail_ShouldReturnConflict(string email)
    {
        // Arrange
        var payload = new RegisterCommand
        {
            Username = Faker.Internet.UserName(),
            Email = email,
            Password = Faker.Internet.StrongPassword()
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

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
            Password = Faker.Internet.StrongPassword()
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

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
        var result = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

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
            Password = Faker.Internet.StrongPassword()
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

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
            Password = Faker.Internet.StrongPassword()
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task Register_WithoutEmailOrUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var payload = new RegisterCommand
        {
            Password = Faker.Internet.StrongPassword()
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

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
        var result = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
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