namespace Depot.Auth.Tests.Auth;

using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Logout;
using Domain.Interfaces;
using Factories;
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
}