namespace Depot.Auth.Tests.Features.Auth.Refresh;

using System.Net;
using Depot.Auth.Features.Auth.Refresh;
using Login;
using Microsoft.IdentityModel.Tokens;

public class RefreshSecurityTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Refresh_WithRevokedToken_ShouldReturnUnauthorized()
    {
        var user = Fixture.Arrange.User.WithSession(x => x.WithRevoked()).Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new RefreshCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var result = await Fixture.Client.Post("api/v1/auth/refresh", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task? Refresh_WithInvalidToken_ShouldReturnNotFound()
    {
        var user = Fixture.Arrange.User.WithSession(x => x.WithRevoked()).Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new RefreshCommand
        {
            RefreshToken = Base64UrlEncoder.Encode(Fixture.Faker.Random.Bytes(32))
        };

        var result = await Fixture.Client.Post("api/v1/auth/refresh", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
}