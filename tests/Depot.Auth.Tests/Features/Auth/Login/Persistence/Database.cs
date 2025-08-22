namespace Depot.Auth.Tests.Features.Auth.Login.Persistence;

using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Login;
using Domain.Auth;
using Utils;

public class Database(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    private const string ValidUsername = "username";

    private const string ValidEmail = "user@example.com";

    [Theory]
    [InlineData(null, ValidEmail)]
    [InlineData(ValidUsername, null)]
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

        var result = await Fixture.Client.Post("api/v1/auth/login", payload).SendAsync();

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(content);

        var id = content.AccessToken.GetClaimValue<Guid>("jti");

        var exists = await Fixture.Database.FindAsync<Session>(new SessionId(id));

        Assert.NotNull(exists);
    }
}