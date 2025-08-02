namespace Depot.Auth.Tests.Auth;

using System.Net;
using System.Net.Http.Json;
using Bogus;
using Domain.Interfaces;
using Domain.Users;
using Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

public class LoginTests : IClassFixture<ComposeFixture>, IAsyncLifetime
{
    private static readonly Faker Faker = new();

    private readonly ComposeFixture _fixture;

    private WebApplicationFactory<Program> _factory = null!;

    public LoginTests(ComposeFixture fixture)
    {
        _fixture = fixture;
    }

    private string Username { get; } = Faker.Internet.Email();

    private string Password { get; } = Faker.Internet.Password();

    private string Email { get; } = Faker.Internet.Email();

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host =>
            {
                host.UseEnvironment("Test");

                host.UseSetting("ConnectionStrings:Auth", _fixture.Auth);
                host.UseSetting("ConnectionStrings:Cache", _fixture.Cache);
            });

        using var scope = _factory.Services.CreateScope();

        var services = scope.ServiceProvider;

        var hasher = services.GetRequiredService<ISecretHasher>();

        var user = User.Create(
            Username,
            new Email(Email),
            new Password(hasher.Hash(Password)),
            DateTime.UtcNow);

        await using var context = services.GetRequiredService<AuthDbContext>();

        await context.Database.EnsureCreatedAsync();

        context.Users.Add(user);

        await context.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
    {
        // Arrange
        var client = _factory.CreateClient();

        var payload = new
        {
            Username,
            Password = Faker.Internet.Password()
        };

        // Act
        var result = await client.PostAsJsonAsync("api/auth/login", payload);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnSession_WhenPasswordIsValid()
    {
        // Arrange
        var client = _factory.CreateClient();

        var payload = new
        {
            Username,
            Password
        };

        // Act
        var result = await client.PostAsJsonAsync("api/auth/login", payload);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}