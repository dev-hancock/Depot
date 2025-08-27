namespace Depot.Auth.Tests.Features.Auth.Login.Persistence;

using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Login;
using Setup;

public class Cache
{
    private const string ValidUsername = "username";

    private const string ValidEmail = "user@example.com";

    [Test]
    [Arguments(null, ValidEmail)]
    [Arguments(ValidUsername, null)]
    public async Task Login_WithValidPayload_ShouldPersistSession(string? username, string? email)
    {
        var db = Database.CreateScope();

        var user = Arrange.User
            .WithUsername(ValidUsername)
            .WithEmail(ValidEmail)
            .Build();

        await db.SeedAsync(user);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Password = user.Password
        };

        var response = await Requests.Post("api/v1/auth/login", payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        var content = await Assert.That(result).IsNotNull();

        // TODO: Re-enable when caching is implemented
        // var id = content!.AccessToken.GetClaimValue("jti");
        //
        // var exists = await Fixture.Cache.GetAsync(id);
        //
        // await Assert.That(exists).IsNotNull();
    }
}