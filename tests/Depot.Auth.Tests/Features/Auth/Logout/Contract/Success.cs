namespace Depot.Auth.Tests.Features.Auth.Logout.Contract;

using System.Net;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Logout;
using Login;

public class Success(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    private const string ValidRefreshToken = "valid-refresh-token";

    [Theory]
    [InlineData(null)]
    [InlineData(ValidRefreshToken)]
    public async Task Logout_WithValidPayload_ShouldReturnNoContent(string? token)
    {
        var user = await Arrange.User
            .WithSession(x => x.WithRefreshToken(ValidRefreshToken))
            .SeedAsync(Services);

        var payload = new LogoutCommand
        {
            RefreshToken = token
        };

        var request = Requests.Post("api/v1/auth/logout", payload, user.Sessions[0].AccessToken);

        var result = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }
}