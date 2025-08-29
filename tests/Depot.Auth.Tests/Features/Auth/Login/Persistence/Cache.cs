using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Login;
using Depot.Auth.Tests.Setup;
using Microsoft.Extensions.Caching.Distributed;

namespace Depot.Auth.Tests.Features.Auth.Login.Persistence;

public class Login_Cache
{
    private static readonly string Id = Unique.Id();

    public static readonly string Username = Unique.Username(Id);

    public static readonly string Email = Unique.Email(Id);

    public static readonly string Password = "Super$ecr3t!";

    public static IEnumerable<(string?, string?)> Data()
    {
        yield return (null, Email);
        yield return (Username, null);
    }

    [Before(Class)]
    public static async Task Setup()
    {
        var user = Arrange.User
            .WithUsername(Username)
            .WithEmail(Email)
            .WithPassword(Password)
            .Build();

        await Database.SeedAsync(user);
    }

    [Test]
    [MethodDataSource(nameof(Data))]
    public async Task Login_WithValidPayload_CachesSession(string? username, string? email)
    {
        var payload = new LoginCommand
        {
            Username = username, Email = email, Password = Password
        };

        var response = await Requests.Post("api/v1/auth/login", payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        var content = await Assert.That(result).IsNotNull();

        var id = content!.AccessToken.GetClaimValue("jti");

        var exists = await Service.Get<IDistributedCache>().GetAsync(id);

        await Assert.That(exists).IsNotNull();
    }
}