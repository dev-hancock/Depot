namespace Depot.Auth.Tests.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Domain.Auth;
using Domain.Interfaces;
using Factories;
using Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

public class RefreshTokenTests : IClassFixture<InfraFixture>, IAsyncLifetime
{
    private readonly InfraFixture _fixture;

    private readonly JwtSecurityTokenHandler _handler = new();

    private IDistributedCache _cache = null!;

    private HttpClient _client = null!;

    private AuthDbContext _db = null!;

    private IServiceScope _scope = null!;

    private ITokenGenerator _tokens = null!;

    public RefreshTokenTests(InfraFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        var factory = new TestAppFactory(_fixture);

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
    public async Task RefreshToken_WithValidPayload_ShouldReturnSession()
    {
        // Arrange
        var user = await _db.Users.FirstOrDefaultAsync();

        Assert.NotNull(user);

        var session = user.CreateSession(_tokens.GenerateRefreshToken(DateTime.UtcNow));

        await _db.SaveChangesAsync();

        var token = _tokens.GenerateAccessToken(user, session.Value.Id, DateTime.UtcNow);

        var payload = new RefreshTokenCommand
        {
            RefreshToken = session.Value.RefreshToken
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/refresh")
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

        await AssertSession(result, session.Value);
    }

    [Fact]
    public async Task RefreshToken_WithRevokedSession_ShouldReturnUnauthorized()
    {
        // Arrange
        var user = await _db.Users.FirstOrDefaultAsync();

        Assert.NotNull(user);

        var session = user.CreateSession(_tokens.GenerateRefreshToken(DateTime.UtcNow));

        await _db.SaveChangesAsync();

        session.Value.Revoke();

        await _db.SaveChangesAsync();

        var token = _tokens.GenerateAccessToken(user, session.Value.Id, DateTime.UtcNow);

        var payload = new RefreshTokenCommand
        {
            RefreshToken = session.Value.RefreshToken
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/refresh")
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
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }


    [Fact]
    public async Task RefreshToken_WithExpiredSession_ShouldReturnUnauthorized()
    {
        // Arrange
        var user = await _db.Users.FirstOrDefaultAsync();

        Assert.NotNull(user);

        var session = user.CreateSession(_tokens.GenerateRefreshToken(DateTime.UtcNow));

        await _db.SaveChangesAsync();

        session.Value.Revoke();

        await _db.SaveChangesAsync();

        var token = _tokens.GenerateAccessToken(user, session.Value.Id, DateTime.UtcNow);

        var payload = new RefreshTokenCommand
        {
            RefreshToken = session.Value.RefreshToken
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/refresh")
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
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidRefreshToken_ShouldReturnNotFound()
    {
        // Arrange
        var user = await _db.Users.FirstOrDefaultAsync();

        Assert.NotNull(user);

        var session = user.CreateSession(_tokens.GenerateRefreshToken(DateTime.UtcNow));

        await _db.SaveChangesAsync();

        var token = _tokens.GenerateAccessToken(user, session.Value.Id, DateTime.UtcNow);

        var payload = new RefreshTokenCommand
        {
            RefreshToken = _tokens.GenerateRefreshToken(DateTime.UtcNow)
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/refresh")
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

    private async Task AssertSession(HttpResponseMessage response, Session session)
    {
        var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        Assert.NotNull(result);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);

        var token = _handler.ReadJwtToken(result.AccessToken);

        var sessionId = token.Claims.SingleOrDefault(x => x.Type == "jti");
        var userId = token.Claims.SingleOrDefault(x => x.Type == "sub");

        Assert.NotNull(sessionId);
        Assert.NotNull(userId);

        Assert.Equal(session.Id.Value.ToString(), sessionId.Value);
        Assert.Equal(session.UserId.Value.ToString(), userId.Value);

        var exists = await _cache.GetAsync(session.Id.Value.ToString());

        Assert.NotNull(exists);
        Assert.Equal([0x1], exists);
    }
}