namespace Depot.Auth.Tests.Features.Auth.Logout;

public class LogoutFailureTests : TestBase
{
    private static async Task AssertProblem(ProblemDetails content)
    {
        await Assert.That(content.Title).IsEqualTo(ReasonPhrases.GetReasonPhrase(401));
        await Assert.That(content.Status).IsEqualTo(401);
        await Assert.That(content.Detail!).IsNotEmpty();
    }

    [Test]
    public async Task Logout_WithoutAccessToken_ReturnsUnauthorized()
    {
        var user = Arrange.User.WithSession().Build();

        await Fixture.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var response = await Api.Logout(payload).SendAsync();

        await Assert.That(response.StatusCode).IsUnauthorized();

        var result = await response.ReadAsAsync<ProblemDetails>();

        var content = await Assert.That(result).IsNotNull();

        await AssertProblem(content);
    }
}