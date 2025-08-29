using Depot.Auth.Features.Auth.Logout;

namespace Depot.Auth.Tests.Features.Auth.Logout;

public class LogoutContractTests
{
    [Test]
    public async Task Logout_WithoutRefreshToken_ShouldReturnOk()
    {
        var user = Arrange.User.WithSession().Build();

        await Database.SeedAsync(user);

        var payload = new LogoutCommand();

        var result = await Requests.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        await Assert.That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Logout_WithRefreshToken_ShouldReturnOk()
    {
        var user = Arrange.User.WithSession().Build();

        await Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var result = await Requests.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        await Assert.That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }
}