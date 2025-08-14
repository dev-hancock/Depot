namespace Depot.Auth.Tests.Features.Auth.Login;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Login;

public class LoginSecurityTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    private readonly JwtSecurityTokenHandler _handler = new();

    [Fact]
    public async Task Login_WithValidPayload_ShouldReturnAccessToken()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Email = user.Email,
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