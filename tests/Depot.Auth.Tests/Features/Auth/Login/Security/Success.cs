namespace Depot.Auth.Tests.Features.Auth.Login.Security;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Login;

public class Success(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    private const string ValidUsername = "username";

    private const string ValidEmail = "user@example.com";

    private readonly JwtSecurityTokenHandler _handler = new();

    [Theory]
    [InlineData(null, ValidEmail)]
    [InlineData(ValidUsername, null)]
    public async Task Login_WithValidPayload_ShouldReturnAccessToken(string? username, string? email)
    {
        var user = await Arrange.User
            .WithUsername(username ?? Faker.Internet.UserName())
            .WithEmail(email ?? Faker.Internet.Email())
            .SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = username,
            Email = email,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
        Assert.NotEmpty(session.AccessToken);
        Assert.NotEmpty(session.RefreshToken);

        var token = _handler.ReadJwtToken(session.AccessToken);

        Assert.Equal(user.Id.ToString(), token.Subject);
    }
}