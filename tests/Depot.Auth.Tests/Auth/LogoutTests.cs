namespace Depot.Auth.Tests.Auth;

using System.Net;
using System.Net.Http.Json;
using Domain.Auth;
using Domain.Interfaces;
using Factories;
using Features.Auth.Logout;
using Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

public class LogoutTests : IClassFixture<InfraFixture>, IAsyncLifetime
{
    private readonly InfraFixture _fixture;

    private IDistributedCache _cache = null!;

    private HttpClient _client = null!;

    private AuthDbContext _db = null!;

    private IServiceScope _scope = null!;

    private ITokenGenerator _tokens = null!;

    public LogoutTests(InfraFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        var factory = new AuthAppFactory(_fixture);

        _client = factory.CreateClient();

        _scope = factory.Services.CreateScope();

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
    public async Task Logout_WithValidPayload_ShouldRevokeSession()
    {
        // Arrange
        var user = await _db.Users.FirstOrDefaultAsync();

        Assert.NotNull(user);

        var session = user.CreateSession(_tokens.GenerateRefreshToken(DateTime.UtcNow));

        var token = _tokens.GenerateAccessToken(user, session.Value.Id, DateTime.UtcNow);

        await _db.SaveChangesAsync();

        var payload = new LogoutCommand
        {
            RefreshToken = session.Value.RefreshToken
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout")
        {
            Content = JsonContent.Create(payload),
            Headers =
            {
                { "Authorization", $"Bearer {token.Value}" }
            }
        };

        // Act
        var result = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        await AssertSessionRevoked(session.Value.Id);
    }

    private async Task AssertSessionExists(SessionId id)
    {
        var exists = await _cache.GetAsync(id.Value.ToString());

        Assert.NotNull(exists);

        var session = await _db.Sessions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);

        Assert.NotNull(session);
        Assert.False(session.IsRevoked);
    }

    private async Task AssertSessionRevoked(SessionId id)
    {
        var exists = await _cache.GetAsync(id.Value.ToString());

        Assert.Null(exists);

        var session = await _db.Sessions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);

        Assert.NotNull(session);
        Assert.True(session.IsRevoked);
    }

    [Fact]
    public async Task Logout_WithoutAccessToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var user = await _db.Users.FirstOrDefaultAsync();

        Assert.NotNull(user);

        var session = user.CreateSession(_tokens.GenerateRefreshToken(DateTime.UtcNow));

        await _db.SaveChangesAsync();

        var payload = new LogoutCommand
        {
            RefreshToken = session.Value.RefreshToken
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout")
        {
            Content = JsonContent.Create(payload)
        };

        // Act
        var result = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);

        await AssertSessionExists(session.Value.Id);
    }

    [Fact]
    public async Task Logout_WithRevokedRefreshToken_ShouldReturnNotFound()
    {
        // Arrange
        var user = await _db.Users.FirstOrDefaultAsync();

        Assert.NotNull(user);

        var session = user.CreateSession(_tokens.GenerateRefreshToken(DateTime.UtcNow));

        session.Value.Revoke();

        var token = _tokens.GenerateAccessToken(user, session.Value.Id, DateTime.UtcNow);

        await _db.SaveChangesAsync();

        var payload = new LogoutCommand
        {
            RefreshToken = session.Value.RefreshToken
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout")
        {
            Content = JsonContent.Create(payload),
            Headers =
            {
                { "Authorization", $"Bearer {token.Value}" }
            }
        };

        // Act
        var result = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task Logout_WithInvalidRefreshToken_ShouldReturnNotFound()
    {
        // Arrange
        var user = await _db.Users.FirstOrDefaultAsync();

        Assert.NotNull(user);

        var session = user.CreateSession(_tokens.GenerateRefreshToken(DateTime.UtcNow));

        await _db.SaveChangesAsync();

        var token = _tokens.GenerateAccessToken(user, session.Value.Id, DateTime.UtcNow);

        var payload = new LogoutCommand
        {
            RefreshToken = _tokens.GenerateRefreshToken(DateTime.UtcNow)
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout")
        {
            Content = JsonContent.Create(payload),
            Headers =
            {
                { "Authorization", $"Bearer {token.Value}" }
            }
        };

        // Act
        var result = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);

        await AssertSessionExists(session.Value.Id);
    }

    [Fact]
    public async Task Logout_WithoutRefreshToken_ShouldRevokeAllSessions()
    {
        // Arrange
        var user = await _db.Users.FirstOrDefaultAsync();

        Assert.NotNull(user);

        for (var i = 0; i < 10; i++)
        {
            _ = user.CreateSession(_tokens.GenerateRefreshToken(DateTime.UtcNow));
        }

        var session = user.CreateSession(_tokens.GenerateRefreshToken(DateTime.UtcNow));

        var token = _tokens.GenerateAccessToken(user, session.Value.Id, DateTime.UtcNow);

        await _db.SaveChangesAsync();

        var payload = new LogoutCommand();

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout")
        {
            Content = JsonContent.Create(payload),
            Headers =
            {
                { "Authorization", $"Bearer {token.Value}" }
            }
        };

        // Act
        var result = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var sessions = await _db.Sessions
            .AsNoTracking()
            .Where(x => x.UserId == user.Id)
            .ToListAsync();

        Assert.All(sessions, x => Assert.True(x.IsRevoked));

        foreach (var id in sessions.Select(x => x.Id))
        {
            var exists = await _cache.GetAsync(id.Value.ToString());

            Assert.Null(exists);
        }
    }
}