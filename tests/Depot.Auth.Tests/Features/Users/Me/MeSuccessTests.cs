namespace Depot.Auth.Tests.Features.Users.Me;

public class MeSuccessTests : TestBase
{
    [Test]
    public async Task Me_WithAccessToken_ReturnsUserDetails()
    {
        var user = Arrange.User.Build();

        await Fixture.SeedAsync(user);

        var response = await Api.Me()
            .Authorize(x => x.WithUser(user))
            .SendAsync();

        await Assert.That(response.StatusCode).IsOk();

        var result = await response.ReadAsAsync<MeResponse>();

        var content = await Assert.That(result).IsNotNull();

        await Assert.That(content.Username).IsEqualTo(user.Username);
    }
}
