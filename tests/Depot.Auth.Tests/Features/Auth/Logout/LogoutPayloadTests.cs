namespace Depot.Auth.Tests.Features.Auth.Logout;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Data;
using Depot.Auth.Features.Auth.Logout;
using Factories;
using Fixtures;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

public sealed class LogoutPayloadTests : IClassFixture<InfraFixture>
{
    private readonly IDistributedCache _cache;

    private readonly HttpClient _client;

    private readonly AuthDbContext _db;

    private readonly TestAppFactory _factory;

    private readonly JwtSecurityTokenHandler _handler = new();

    public LogoutPayloadTests(InfraFixture fixture)
    {
        _factory = new TestAppFactory(fixture);

        _client = _factory.CreateClient();

        _cache = _factory.Services.GetRequiredService<IDistributedCache>();

        _db = _factory.Services.GetRequiredService<AuthDbContext>();
    }

    [Fact]
    public async Task Logout_WithoutAccessToken_ShouldReturnUnauthorized()
    {
        var user = await Arrange.User(x => x.WithSession()).SeedAsync(_factory.Services);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout")
        {
            Content = JsonContent.Create(payload)
        };

        var result = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task Logout_WithRevokedRefreshToken_ShouldReturnNotFound()
    {
        var user = await Arrange.User(x => x.WithSession()).SeedAsync(_factory.Services);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout")
        {
            Content = JsonContent.Create(payload),
            Headers =
            {
                { "Authorization", $"Bearer {user.Sessions[0].AccessToken}" }
            }
        };

        var result = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }
}