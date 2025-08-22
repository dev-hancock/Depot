namespace Depot.Auth.Tests.Features.Auth.Logout.Contract;

using System.Net;
using Depot.Auth.Features.Auth.Logout;
using Login;

public class Success(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    private const string ValidRefreshToken = "valid-refresh-token";

    [Theory]
    [InlineData(null)]
    [InlineData(ValidRefreshToken)]
    public async Task Logout_WithValidPayload_ShouldReturnNoContent(string? token)
    {
        var user = Fixture.Arrange.User
            .WithSession(x => x.WithRefreshToken(ValidRefreshToken))
            .Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new LogoutCommand
        {
            RefreshToken = token
        };

        var result = await Fixture.Client.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(user.Id.Value))
            .SendAsync();

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }
}