using Depot.Auth.Features.Auth.Refresh;

namespace Depot.Auth.Tests.Features.Auth.Refresh;

public class RefreshFailureTests : TestBase
{
    private static async Task AssertProblem(ProblemDetails content)
    {
        await Assert.That(content.Title).IsEqualTo(ReasonPhrases.GetReasonPhrase(401));
        await Assert.That(content.Status).IsEqualTo(401);
        await Assert.That(content.Detail!).IsNotEmpty();
    }

    private static async Task AssertResponse(HttpResponseMessage response)
    {
        await Assert.That(response.StatusCode).IsUnauthorized();

        var result = await response.ReadAsAsync<ProblemDetails>();

        var content = await Assert.That(result).IsNotNull();

        await AssertProblem(content);
    }

    [Test]
    public async Task? Refresh_WithExpiredToken_ReturnsUnauthorized()
    {
        var user = Arrange.User
            .WithSession(x =>
                x.WithExpiry(DateTime.UtcNow.AddHours(-1)))
            .Build();

        await Fixture.SeedAsync(user);

        var payload = new RefreshCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var response = await Api.Refresh(payload)
            .Authorize(x => x.WithUser(user.Id.Value))
            .SendAsync();

        await AssertResponse(response);
    }

    [Test]
    public async Task? Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        var user = Arrange.User.WithSession().Build();

        var token = Arrange.RefreshToken.Build();

        await Fixture.SeedAsync(user);

        var payload = new RefreshCommand
        {
            RefreshToken = token
        };

        var response = await Api.Refresh(payload)
            .Authorize(x => x.WithUser(user.Id.Value))
            .SendAsync();

        await AssertResponse(response);
    }

    [Test]
    public async Task Refresh_WithoutSession_ReturnsUnauthorized()
    {
        var user = Arrange.User.Build();

        var token = Arrange.RefreshToken.Build();

        await Fixture.SeedAsync(user);

        var payload = new RefreshCommand
        {
            RefreshToken = token
        };

        var response = await Api.Refresh(payload)
            .Authorize(x => x.WithUser(user.Id.Value))
            .SendAsync();

        await AssertResponse(response);
    }

    [Test]
    public async Task Refresh_WithRevokedToken_ReturnsUnauthorized()
    {
        var user = Arrange.User.WithSession(x => x.WithRevoked()).Build();

        await Fixture.SeedAsync(user);

        var payload = new RefreshCommand
        {
            RefreshToken = user.Sessions[0].RefreshToken
        };

        var response = await Api.Refresh(payload)
            .Authorize(x => x.WithUser(user.Id.Value))
            .SendAsync();

        await AssertResponse(response);
    }
}