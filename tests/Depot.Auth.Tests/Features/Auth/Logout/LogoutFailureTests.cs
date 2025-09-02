namespace Depot.Auth.Tests.Features.Auth.Logout;

public class LogoutFailureTests
{
    [Test]
    public async Task Logout_WithoutAccessToken_ReturnsUnauthorized()
    {
        var user = Arrange.User.WithSession().Build();

        await Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var response = await Requests.Logout(payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }
}