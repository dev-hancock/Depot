namespace Depot.Auth.Tests.Features.Auth.Logout.Contract;

public class Logout_Success
{
    private const string RefershToken = "valid-refresh-token";

    private static readonly Guid Id = Guid.NewGuid();

    public static IEnumerable<string?> Data()
    {
        yield return null;
        yield return RefershToken;
    }

    [Before(Class)]
    public static async Task Setup()
    {
        var user = Arrange.User
            .WithId(Id)
            .WithSession(x => x.WithRefreshToken(RefershToken))
            .Build();

        await Database.SeedAsync(user);
    }

    [Test]
    [MethodDataSource(nameof(Data))]
    public async Task Logout_WithValidPayload_ShouldReturnNoContent(string? token)
    {
        var payload = new LogoutCommand
        {
            RefreshToken = token
        };

        var response = await Requests.Post("api/v1/auth/logout", payload)
            .WithAuthorization(x => x.WithUser(Id))
            .SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }
}