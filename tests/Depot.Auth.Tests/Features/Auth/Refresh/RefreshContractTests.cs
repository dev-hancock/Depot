namespace Depot.Auth.Tests.Features.Auth.Refresh;

using System.Net;
using System.Net.Http.Json;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Refresh;
using Login;

public class RefreshContractTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Refresh_WithValidToken_ShouldReturnSession()
    {
        var user = await Arrange.User.WithSession().SeedAsync(Services);

        var payload = new RefreshCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var request = Requests.Post("api/v1/auth/refresh", payload, user.Sessions[0].AccessToken);

        var result = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<RefreshResponse>();

        Assert.NotNull(session);
    }

    [Fact]
    public async Task Refresh_WithoutToken_ShouldReturnBadRequest()
    {
        var user = await Arrange.User.WithSession().SeedAsync(Services);

        var payload = new RefreshCommand();

        var request = Requests.Post("api/v1/auth/refresh", payload, user.Sessions[0].AccessToken);

        var result = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<RefreshResponse>();

        Assert.Null(session);
    }
}