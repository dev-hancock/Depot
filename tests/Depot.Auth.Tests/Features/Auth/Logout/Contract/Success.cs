using Depot.Auth.Features.Auth.Logout;

namespace Depot.Auth.Tests.Features.Auth.Logout.Contract;

public class Success
{
    private const string ValidRefreshToken = "valid-refresh-token";

    [Test]
    [Arguments(null)]
    [Arguments(ValidRefreshToken)]
    public async Task Logout_WithValidPayload_ShouldReturnNoContent(string? token)
    {
        var user = Arrange.User
            .WithSession(x => x.WithRefreshToken(ValidRefreshToken))
            .Build();

        await Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = token
        };

        var response = await Requests.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }
}