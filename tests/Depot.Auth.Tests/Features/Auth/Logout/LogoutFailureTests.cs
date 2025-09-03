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

        var result = await response.ReadAsAsync<ProblemDetails>();

        var content = await Assert.That(result).IsNotNull();

        await Assert.That(content.Title).IsEqualTo(ReasonPhrases.GetReasonPhrase(401));
        await Assert.That(content.Status).IsEqualTo(401);
        await Assert.That(content.Detail!).IsNotEmpty();
    }
}