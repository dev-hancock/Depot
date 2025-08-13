namespace Depot.Auth.Tests.Features.Auth.Logout;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Data;
using Depot.Auth.Features.Auth.Logout;
using Domain.Auth;
using Factories;
using Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

public sealed class LogoutSessionTests : IClassFixture<InfraFixture>
{
    private readonly IDistributedCache _cache;

    private readonly HttpClient _client;

    private readonly AuthDbContext _db;

    private readonly TestAppFactory _factory;

    private readonly JwtSecurityTokenHandler _handler = new();

    public LogoutSessionTests(InfraFixture fixture)
    {
        _factory = new TestAppFactory(fixture);

        _client = _factory.CreateClient();

        _cache = _factory.Services.GetRequiredService<IDistributedCache>();

        _db = _factory.Services.GetRequiredService<AuthDbContext>();
    }

    [Fact]
    public async Task Logout_WithRefreshToken_ShouldRevokeSession()
    {
        var user = await Arrange.User
            .WithSession()
            .WithSession()
            .WithSession()
            .SeedAsync(_factory.Services);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var request = CreateRequest(payload, user.Sessions[0].AccessToken);

        var result = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var exists = await _cache.GetAsync(user.Sessions[0].Id.ToString());

        Assert.NotNull(exists);

        var entity = await _db.Sessions.FindAsync(user.Sessions[0].Id);

        Assert.NotNull(entity);
        Assert.True(entity.IsRevoked);
    }

    private HttpRequestMessage CreateRequest(LogoutCommand command, string token)
    {
        return new HttpRequestMessage(HttpMethod.Post, "api/auth/logout")
        {
            Content = JsonContent.Create(command),
            Headers =
            {
                { "Authorization", $"Bearer {token}" }
            }
        };
    }

    [Fact]
    public async Task Logout_WithoutRefreshToken_ShouldRevokeAllSessions()
    {
        var user = await Arrange.User
            .WithSession()
            .WithSession()
            .WithSession()
            .SeedAsync(_factory.Services);

        var payload = new LogoutCommand();

        var request = CreateRequest(payload, user.Sessions[0].AccessToken);

        var result = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var sessions = await _db.Sessions
            .Where(x => x.UserId == new UserId(user.Id))
            .ToListAsync();

        Assert.All(sessions, x => Assert.True(x.IsRevoked));

        foreach (var id in sessions.Select(x => x.Id))
        {
            var exists = await _cache.GetAsync(id.Value.ToString());

            Assert.Null(exists);
        }
    }
}