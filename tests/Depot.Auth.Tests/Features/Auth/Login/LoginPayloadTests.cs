namespace Depot.Auth.Tests.Features.Auth.Login;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Data;
using Depot.Auth.Features.Auth.Login;
using Domain.Auth;
using Factories;
using Fixtures;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

public sealed class LoginPayloadTests : IClassFixture<InfraFixture>
{
    private readonly IDistributedCache _cache;

    private readonly HttpClient _client;

    private readonly AuthDbContext _db;

    private readonly TestAppFactory _factory;

    private readonly JwtSecurityTokenHandler _handler = new();

    public LoginPayloadTests(InfraFixture fixture)
    {
        _factory = new TestAppFactory(fixture);

        _client = _factory.CreateClient();

        _cache = _factory.Services.GetRequiredService<IDistributedCache>();

        _db = _factory.Services.GetRequiredService<AuthDbContext>();
    }

    [Fact]
    public async Task Login_WithValidPayload_ShouldCacheSession()
    {
        var user = await Arrange.User().SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Email = user.Email,
            Password = user.Password
        };

        var result = await _client.PostAsJsonAsync("/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
        Assert.NotNull(session.AccessToken);
        Assert.NotNull(session.RefreshToken);

        var token = _handler.ReadJwtToken(session.AccessToken);

        var id = token.Claims.SingleOrDefault(x => x.Type == "jti");

        Assert.NotNull(id);

        var exists = await _cache.GetAsync(id.Value);

        Assert.NotNull(exists);
    }

    [Fact]
    public async Task Login_WithValidPayload_ShouldPersistSession()
    {
        var user = await Arrange.User().SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Email = user.Email,
            Password = user.Password
        };

        var result = await _client.PostAsJsonAsync("/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
        Assert.NotNull(session.AccessToken);
        Assert.NotNull(session.RefreshToken);

        var token = _handler.ReadJwtToken(session.AccessToken);

        var id = token.Claims.SingleOrDefault(x => x.Type == "jti");

        Assert.NotNull(id);

        var exists = await _db.Sessions.FindAsync(new SessionId(Guid.Parse(id.Value)));

        Assert.NotNull(exists);
    }

    [Fact]
    public async Task Login_WithEmptyPayload_ShouldReturnBadRequest()
    {
        var payload = new LoginCommand();

        var result = await _client.PostAsJsonAsync("/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }
}