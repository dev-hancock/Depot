namespace Depot.Auth.Tests.Features.Auth.Logout;

using System.Net;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Logout;
using Login;

public class LogoutContractTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Logout_WithRefreshToken_ShouldReturnOk()
    {
        var user = await Arrange.User.WithSession().SeedAsync(Services);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var request = Requests.Post("api/v1/auth/logout", payload, user.Sessions[0].AccessToken);

        var result = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutRefreshToken_ShouldReturnOk()
    {
        var user = await Arrange.User.WithSession().SeedAsync(Services);

        var payload = new LogoutCommand();

        var request = Requests.Post("api/v1/auth/logout", payload, user.Sessions[0].AccessToken);

        var result = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}