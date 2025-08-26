namespace Depot.Auth.Tests.Features.Auth.Login.Contract;

using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Login;

public static class Unique
{
    public static string Email(string user, string suffix)
    {
        return $"{user}-{Guid.NewGuid()}{suffix}";
    }

    public static string Username(string user)
    {
        return $"{user}-{Guid.NewGuid()}";
    }
}

public class LoginContractSuccess : IntegrationTest
{
    private const string User = "user";

    private const string EmailSuffix = "@example.com";

    private const string Password = "Sup3r$ecret!";

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
            var email = Unique.Email(User, EmailSuffix);
            var username = Unique.Username(User);

            yield return new LoginCase(username, email, variant(email));
        }
    }

    [Test]
    [MethodDataSource(nameof(EmailVariants))]
    public async Task Login_WithValidEmail_ShouldReturnSession(LoginCase c)
    {
        var user = Fixture.Arrange.User
            .WithUsername(c.Username)
            .WithEmail(c.Email)
            .WithPassword(Password)
            .Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new LoginCommand
        {
            Email = c.Variant,
            Password = Password
        };

        var response = await Fixture.Client.Post("api/v1/auth/login", payload).SendAsync();

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