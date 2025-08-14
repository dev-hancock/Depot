namespace Depot.Auth.Tests.Features.Auth.Login;

using System.Net;
using System.Net.Http.Json;
using Data;
using Depot.Auth.Features.Auth.Login;

public class LoginPersistenceTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Login_WithValidPayload_ShouldPersistSession()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Email = user.Email,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var exists = await Db.Sessions.FindAsync(user.Sessions[0].Id);

        Assert.NotNull(exists);
    }

    [Fact]
    public async Task Login_WithValidPayload_ShouldCacheSession()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Email = user.Email,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var exists = await Cache.GetAsync(user.Sessions[0].Id.ToString());

        Assert.NotNull(exists);
    }
}