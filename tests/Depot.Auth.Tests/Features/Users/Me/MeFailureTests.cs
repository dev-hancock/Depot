namespace Depot.Auth.Tests.Features.Users.Me;

public class MeFailureTests : TestBase
{
    [Test]
    public async Task Me_WithoutAccessToken_ReturnsUnauthorized()
    {
        var user = Arrange.User.Build();

        await Fixture.SeedAsync(user);

        var response = await Api.Me().SendAsync();

        await Assert.That(response.StatusCode).IsUnauthorized();

        var result = await response.ReadAsAsync<ProblemDetails>();

        var content = await Assert.That(result).IsNotNull();

        await AssertProblem(content);
    }

    private static async Task AssertProblem(ProblemDetails content)
    {
        await Assert.That(content.Title).IsEqualTo(ReasonPhrases.GetReasonPhrase(401));
        await Assert.That(content.Status).IsEqualTo(401);
        await Assert.That(content.Detail).IsNotEmpty();
    }
}