namespace Depot.Auth.Tests.Features.Auth.Logout;

using System.Net;
using System.Net.Http.Json;
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

        var request = CreateRequest(payload, user.Sessions[0].AccessToken);

        var result = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    private static HttpRequestMessage CreateRequest(LogoutCommand command, string token)
    {
        return new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/logout")
        {
            Content = JsonContent.Create(command),
            Headers =
            {
                { "Authorization", $"Bearer {token}" }
            }
        };
    }

    [Fact]
    public async Task Logout_WithoutRefreshToken_ShouldReturnOk()
    {
        var user = await Arrange.User.WithSession().SeedAsync(Services);

        var payload = new LogoutCommand();

        var request = CreateRequest(payload, user.Sessions[0].AccessToken);

        var result = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}