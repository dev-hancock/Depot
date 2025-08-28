namespace Depot.Auth.Tests.Features.Auth.Login.Security;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Login;
using Setup;

public class Success
{
    private const string ValidUsername = "username";

    private const string ValidEmail = "user@example.com";

    private readonly JwtSecurityTokenHandler _handler = new();

    [Test]
    [Arguments(null, ValidEmail)]
    [Arguments(ValidUsername, null)]
    public async Task Login_WithValidPayload_ShouldReturnAccessToken(string? username, string? email)
    {
        var user = Arrange.User
            .WithUsername(ValidUsername)
            .WithEmail(ValidEmail)
            .Build();

        await Database.SeedAsync(user);

        var payload = new LoginCommand
        {
            Username = username,
            Email = email,
            Password = user.Password
        };

        var response = await Requests.Post("api/v1/auth/login", payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        var session = await Assert.That(result).IsNotNull();

        await Assert.That(session.AccessToken).IsNotNull();
        await Assert.That(session.RefreshToken).IsNotEmpty();

        var token = _handler.ReadJwtToken(session.AccessToken);

        await Assert.That(token.Subject).IsEqualTo(user.Id.ToString());
    }
}