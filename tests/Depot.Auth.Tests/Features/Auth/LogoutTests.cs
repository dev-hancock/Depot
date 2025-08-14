namespace Depot.Auth.Tests.Auth;

using System.Net;
using System.Net.Http.Json;
using Bogus;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Logout;
using Domain.Interfaces;
using Factories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Persistence;

public class LogoutTests : IClassFixture<InfraFixture>, IAsyncLifetime
{
    private static readonly Faker Faker = new();

    private readonly InfraFixture _fixture;

    private IDistributedCache _cache = null!;

    private HttpClient _client = null!;

    private AuthDbContext _db = null!;

    private TestAppFactory _factory;

    private IServiceScope _scope = null!;

    private ITokenGenerator _tokens = null!;

    public LogoutTests(InfraFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        _factory = new TestAppFactory(_fixture);

        _client = _factory.CreateClient();

        _scope = _factory.Services.CreateScope();

        _db = _scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        _cache = _scope.ServiceProvider.GetRequiredService<IDistributedCache>();

        _tokens = _scope.ServiceProvider.GetRequiredService<ITokenGenerator>();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }


    [Fact]
    public async Task Logout_WithInvalidRefreshToken_ShouldReturnNotFound()
    {
        var user = await Arrange.User.WithSession().SeedAsync(_factory.Services);

        var payload = new LogoutCommand
        {
            RefreshToken = Base64UrlEncoder.Encode(Faker.Random.Bytes(32))
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout")
        {
            Content = JsonContent.Create(payload),
            Headers =
            {
                { "Authorization", $"Bearer {user.Sessions[0].AccessToken}" }
            }
        };

        // Act
        var result = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }
}