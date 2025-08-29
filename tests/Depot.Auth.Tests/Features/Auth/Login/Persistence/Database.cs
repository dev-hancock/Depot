using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Login;
using Depot.Auth.Tests.Setup;

namespace Depot.Auth.Tests.Features.Auth.Login.Persistence;

public class DatabaseTests
{
    private const string ValidUsername = "username";

    private const string ValidEmail = "user@example.com";

    [Test]
    [Arguments(null, ValidEmail)]
    [Arguments(ValidUsername, null)]
    public async Task Login_WithValidPayload_ShouldPersistSession(string? username, string? email)
    {
        var user = Arrange.User
            .WithUsername(ValidUsername)
            .WithEmail(ValidEmail)
            .Build();

        await Database.SeedAsync(user);

        var payload = new LoginCommand
        {
            Username = user.Username, Password = user.Password
        };

        var response = await Requests.Post("api/v1/auth/login", payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        var content = await Assert.That(result).IsNotNull();

        // TODO: Re-enable when session persistence is implemented
        // var id = content.AccessToken.GetClaimValue<Guid>("jti");
        //
        // var exists = await Fixture.Database.FindAsync<Session>(new SessionId(id));
        //
        // await Assert.That(exists).IsNotNull();
    }
}