using Depot.Auth.Features.Auth.Refresh;

namespace Depot.Auth.Tests.Features.Auth.Refresh;

public class RefreshValidationTests : TestBase
{
    private static async Task AssertProblem(ProblemDetails content)
    {
        await Assert.That(content.Title).IsEqualTo(ReasonPhrases.GetReasonPhrase(400));
        await Assert.That(content.Status).IsEqualTo(400);
        await Assert.That(content.Detail!).IsNotEmpty();
    }

    [Test]
    [Arguments("  ")]
    [Arguments("")]
    [Arguments(null)]
    public async Task? Refresh_WithInvalidToken_ReturnsUnauthorized(string? token)
    {
        var user = Arrange.User.WithSession().Build();

        await Fixture.SeedAsync(user);

        var payload = new RefreshCommand
        {
            RefreshToken = token!
        };

        var response = await Api.Refresh(payload)
            .Authorize(x => x.WithUser(user.Id.Value))
            .SendAsync();

        await Assert.That(response.StatusCode).IsBadRequest();

        var result = await response.ReadAsAsync<ProblemDetails>();

        var content = await Assert.That(result).IsNotNull();

        await AssertProblem(content);
    }
}