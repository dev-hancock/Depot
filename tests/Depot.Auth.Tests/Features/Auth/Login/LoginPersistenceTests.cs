namespace Depot.Auth.Tests.Features.Auth.Login;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Login;
using Domain.Auth;

public class LoginPersistenceTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Login_WithUsername_ShouldPersistSession()
    {
        var user = await Arrange.User.WithSession().SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(content);

        var id = content.AccessToken.GetClaimValue("jti");

        var exists = await Db.Sessions.FindAsync(new SessionId(Guid.Parse(id)));

        Assert.NotNull(exists);
    }

    [Fact]
    public async Task Login_WithEmail_ShouldPersistSession()
    {
        var user = await Arrange.User.WithSession().SeedAsync(Services);

        var payload = new LoginCommand
        {
            Email = user.Email,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(content);

        var id = content.AccessToken.GetClaimValue("jti");

        var exists = await Db.Sessions.FindAsync(new SessionId(Guid.Parse(id)));

        Assert.NotNull(exists);
    }

    [Fact]
    public async Task Login_WithUsername_ShouldCacheSession()
    {
        var user = await Arrange.User.WithSession().SeedAsync(Services);

        var payload = new LoginCommand
        {
            Email = user.Email,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(content);

        var id = content.AccessToken.GetClaimValue("jti");

        var exists = await Cache.GetAsync(id);

        Assert.NotNull(exists);
    }

    [Fact]
    public async Task Login_WithEmail_ShouldCacheSession()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Email = user.Email,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(content);

        var id = content.AccessToken.GetClaimValue("jti");

        var exists = await Cache.GetAsync(id);

        Assert.NotNull(exists);
    }
}

public static class JwtTokenExtensions
{
    private static readonly JwtSecurityTokenHandler Handler = new();

    public static string GetClaimValue(this string token, string type)
    {
        return Handler.ReadJwtToken(token).Claims.Single(x => x.Type == type).Value;
    }
}