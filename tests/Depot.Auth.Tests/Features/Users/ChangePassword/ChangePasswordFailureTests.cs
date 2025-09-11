namespace Depot.Auth.Tests.Features.Users.ChangePassword;

public class ChangePasswordFailureTests : TestBase
{
    private static readonly Faker Faker = new();

    private static readonly string NewPassword = Faker.Internet.StrongPassword();

    private static readonly string OldPassword = Faker.Internet.StrongPassword();

    [Test]
    public async Task ChangePassword_WithoutAccessToken_ReturnsUnauthorized()
    {
        var user = Arrange.User.Build();

        await Fixture.SeedAsync(user);

        var payload = new ChangePasswordCommand
        {
            NewPassword = NewPassword,
            OldPassword = OldPassword
        };

        var response = await Api.ChangePassword(payload)
            .Authorize(x => x.WithUser(user))
            .SendAsync();

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
