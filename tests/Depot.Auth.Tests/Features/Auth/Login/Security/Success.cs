namespace Depot.Auth.Tests.Features.Auth.Login.Security;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
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
        var user = Fixture.Arrange.User
            .WithUsername(username ?? Fixture.Faker.Internet.UserName())
            .WithEmail(email ?? Fixture.Faker.Internet.Email())
            .Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new LoginCommand
        {
            Username = username,
            Email = email,
            Password = user.Password
        };

        var result = await Fixture.Client.Post("api/v1/auth/login", payload).SendAsync();

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
        Assert.NotEmpty(session.AccessToken);
        Assert.NotEmpty(session.RefreshToken);

        var token = _handler.ReadJwtToken(session.AccessToken);

        Assert.Equal(user.Id.ToString(), token.Subject);
    }
}