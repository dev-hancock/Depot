namespace Depot.Auth.Tests.Features.Auth.Login.Persistence;

using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Login;
using Utils;

[ClassDataSource(typeof(IntegrationFixture))]
public class Cache : IntegrationTest
{
    private const string ValidUsername = "username";

    private const string ValidEmail = "user@example.com";

    [Test]
    [Arguments(null, ValidEmail)]
    [Arguments(ValidUsername, null)]
    public async Task Login_WithValidPayload_ShouldPersistSession(string? username, string? email)
    {
        var user = Fixture.Arrange.User
            .WithUsername(username ?? Fixture.Faker.Internet.UserName())
            .WithEmail(email ?? Fixture.Faker.Internet.Email())
            .Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Password = user.Password
        };

        var response = await Fixture.Client.Post("api/v1/auth/login", payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        var content = await Assert.That(result).IsNotNull();

        var id = content!.AccessToken.GetClaimValue("jti");

        var exists = await Fixture.Cache.GetAsync(id);

        await Assert.That(exists).IsNotNull();
    }
}