namespace Depot.Auth.Tests.Features.Auth.Refresh;

using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Refresh;
using Login;

public class RefreshContractTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Refresh_WithValidToken_ShouldReturnSession()
    {
        var user = Fixture.Arrange.User.WithSession().Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new RefreshCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var result = await Fixture.Client.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<RefreshResponse>();

        Assert.NotNull(session);
    }

    [Fact]
    public async Task Refresh_WithoutToken_ShouldReturnBadRequest()
    {
        var user = Fixture.Arrange.User.WithSession().Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new RefreshCommand();

        var result = await Fixture.Client.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<RefreshResponse>();

        Assert.Null(session);
    }
}