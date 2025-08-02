namespace Depot.Auth.Tests.Auth;

using System.IdentityModel.Tokens.Jwt;
using Bogus;
using Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

public class RefreshTokenTests : IClassFixture<InfraFixture>, IAsyncLifetime
{
    private static readonly Faker Faker = new();

    private readonly InfraFixture _fixture;

    private readonly JwtSecurityTokenHandler _handler = new();

    private IDistributedCache _cache = null!;

    private HttpClient _client = null!;

    public RefreshTokenTests(InfraFixture fixture)
    {
        _fixture = fixture;
    }

    private string Username { get; } = Faker.Internet.Email();

    private string Password { get; } = Faker.Internet.Password();

    private string Email { get; } = Faker.Internet.Email();

    public Task InitializeAsync()
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

        _cache = services.GetRequiredService<IDistributedCache>();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}