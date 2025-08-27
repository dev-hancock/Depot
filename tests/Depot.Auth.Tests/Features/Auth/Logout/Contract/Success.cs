namespace Depot.Auth.Tests.Features.Auth.Logout.Contract;

using System.Net;
using Depot.Auth.Features.Auth.Logout;
using Setup;

public class Success
{
    private const string ValidRefreshToken = "valid-refresh-token";

    [Test]
    [Arguments(null)]
    [Arguments(ValidRefreshToken)]
    public async Task Logout_WithValidPayload_ShouldReturnNoContent(string? token)
    {
        using var db = Database.CreateScope();

        var user = Arrange.User
            .WithSession(x => x.WithRefreshToken(ValidRefreshToken))
            .Build();

        await db.SeedAsync(user);

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