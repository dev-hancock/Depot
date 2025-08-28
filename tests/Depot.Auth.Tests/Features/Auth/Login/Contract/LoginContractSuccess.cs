namespace Depot.Auth.Tests.Features.Auth.Login.Contract;

using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Login;
using Setup;

public class LoginContractSuccess
{
    private static readonly string Username = "user";

    private static readonly string Email = "user@example.com";

    private static readonly string Password = "Sup3r$ecret!";

    private static IEnumerable<Func<string, string>> Variants()
    {
        yield return x => x;
        yield return x => x.ToUpperInvariant();
        yield return x => x.ToLowerInvariant();
        yield return x => $" {x} ";
        yield return x => $"\t{x}\t";
        yield return x => $"{x}\n";
    }

    public static IEnumerable<LoginCase> EmailVariants()
    {
        foreach (var variant in Variants())
        {
            yield return new LoginCase(Username, Email, variant(Email));
        }
    }

    [Before(Class)]
    public async static Task Setup()
    {
        var user = Arrange.User
            .WithUsername(Username)
            .WithEmail(Email)
            .WithPassword(Password)
            .Build();

        await Database.SeedAsync(user);
    }

    [Test]
    [MethodDataSource(nameof(EmailVariants))]
    public async Task Login_WithValidEmail_ShouldReturnSession(LoginCase c)
    {
        var payload = new LoginCommand
        {
            Email = c.Variant,
            Password = Password
        };

        var response = await Requests.Post("api/v1/auth/login", payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        var session = await Assert.That(result).IsNotNull();

        await Assert.That(session.AccessToken).IsNotEmpty();
        await Assert.That(session.RefreshToken).IsNotEmpty();
    }
    //
    // [Test]
    // public async Task Login_WithValidUsername_ShouldReturnSession(LoginCase c)
    // {
    //     var password = Fixture.Faker.Internet.StrongPassword();
    //
    //     var user = Fixture.Arrange.User
    //         .WithUsername(c.Username)
    //         .WithEmail(c.Email)
    //         .WithPassword(password)
    //         .Build();
    //
    //     await Fixture.Database.SeedAsync(user);
    //
    //     var payload = new LoginCommand
    //     {
    //         Username = c.Variant,
    //         Password = password
    //     };
    //
    //     var response = await Fixture.Client.Post("api/v1/auth/login", payload).SendAsync();
    //
    //     await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    //
    //     var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
    //
    //     var session = await Assert.That(result).IsNotNull();
    //
    //     await Assert.That(session.AccessToken).IsNotEmpty();
    //     await Assert.That(session.RefreshToken).IsNotEmpty();
    // }

    public record LoginCase(string Username, string Email, string Variant)
    {
        public override string ToString()
        {
            return $"Login - Username: {Username}, Email: {Email}";
        }
    }
}