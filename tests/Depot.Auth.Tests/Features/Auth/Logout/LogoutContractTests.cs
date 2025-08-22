namespace Depot.Auth.Tests.Features.Auth.Logout;

using System.Net;
using Depot.Auth.Features.Auth.Logout;
using Login;

public class LogoutContractTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Logout_WithRefreshToken_ShouldReturnOk()
    {
        var user = Fixture.Arrange.User.WithSession().Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var result = await Fixture.Client.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutRefreshToken_ShouldReturnOk()
    {
        var user = Fixture.Arrange.User.WithSession().Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new LogoutCommand();

        var result = await Fixture.Client.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}