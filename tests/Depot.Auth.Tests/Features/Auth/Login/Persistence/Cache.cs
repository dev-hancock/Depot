namespace Depot.Auth.Tests.Features.Auth.Login.Persistence;

using System.Net;
using System.Net.Http.Json;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Login;
using Utils;

public class Cache(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    private const string ValidUsername = "username";

    private const string ValidEmail = "user@example.com";

    [Theory]
    [InlineData(null, ValidEmail)]
    [InlineData(ValidUsername, null)]
    public async Task Login_WithValidPayload_ShouldPersistSession(string? username, string? email)
    {
        var user = await Arrange.User
            .WithUsername(username ?? Faker.Internet.UserName())
            .WithEmail(email ?? Faker.Internet.Email())
            .SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(content);

        var id = content.AccessToken.GetClaimValue("jti");

        var exists = await Cache.GetAsync(id);

        Assert.NotNull(exists);
    }
}