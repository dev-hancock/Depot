namespace Depot.Auth.Tests.Features.Auth.Refresh;

using System.Net;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Refresh;
using Login;
using Microsoft.IdentityModel.Tokens;

public class RefreshSecurityTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Refresh_WithRevokedToken_ShouldReturnUnauthorized()
    {
        var user = await Arrange.User.WithSession(x => x.WithRevoked()).SeedAsync(Services);

        var payload = new RefreshCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var request = Requests.Post("api/v1/auth/refresh", payload, user.Sessions[0].AccessToken);

        var result = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task? Refresh_WithInvalidToken_ShouldReturnNotFound()
    {
        var user = await Arrange.User.WithSession(x => x.WithRevoked()).SeedAsync(Services);

        var payload = new RefreshCommand
        {
            RefreshToken = Base64UrlEncoder.Encode(Faker.Random.Bytes(32))
        };

        var request = Requests.Post("api/v1/auth/refresh", payload, user.Sessions[0].AccessToken);

        var result = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
}