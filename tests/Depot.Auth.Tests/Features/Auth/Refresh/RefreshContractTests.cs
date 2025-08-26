namespace Depot.Auth.Tests.Features.Auth.Refresh;

using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Refresh;

[ClassDataSource(typeof(IntegrationFixture))]
public class RefreshContractTests : IntegrationTest
{
    [Test]
    public async Task Refresh_WithValidToken_ShouldReturnSession()
    {
        var user = Fixture.Arrange.User.WithSession().Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new RefreshCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var response = await Fixture.Client.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RefreshResponse>();

        var session = await Assert.That(result).IsNotNull();
    }

    [Test]
    public async Task Refresh_WithoutToken_ShouldReturnBadRequest()
    {
        var user = Fixture.Arrange.User.WithSession().Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new RefreshCommand();

        var response = await Fixture.Client.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<RefreshResponse>();

        _ = await Assert.That(result).IsNotNull();
    }
}