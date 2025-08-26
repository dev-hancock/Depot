namespace Depot.Auth.Tests.Features.Auth.Logout.Contract;

using System.Net;
using Depot.Auth.Features.Auth.Logout;

[ClassDataSource(typeof(IntegrationFixture))]
public class Success : IntegrationTest
{
    private const string ValidRefreshToken = "valid-refresh-token";

    [Test]
    [Arguments(null)]
    [Arguments(ValidRefreshToken)]
    public async Task Logout_WithValidPayload_ShouldReturnNoContent(string? token)
    {
        var user = Fixture.Arrange.User
            .WithSession(x => x.WithRefreshToken(ValidRefreshToken))
            .Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = token
        };

        var response = await Fixture.Client.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }
}