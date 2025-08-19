namespace Depot.Auth.Tests.Features.Auth.Login.Contract;

using System.Net;
using System.Net.Http.Json;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Login;

public class Success(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    public static IEnumerable<object[]> EquivalentUsernames => Equivalents(Faker.Internet.UserName());

    public static IEnumerable<object[]> EquivalentEmails => Equivalents(Faker.Internet.Email());

    private static IEnumerable<string> Variants(string value)
    {
        yield return value;
        yield return value.ToUpperInvariant();
        yield return value.ToLowerInvariant();
        yield return $" {value} ";
        yield return $"\t{value}\t";
        yield return $"{value}\n";
    }

    private static IEnumerable<object[]> Equivalents(string value)
    {
        return Variants(value).Select(variant => new object[] { value, variant });
    }

    [Theory]
    [MemberData(nameof(EquivalentEmails))]
    public async Task Login_WithValidEmail_ShouldReturnSession(string value, string variant)
    {
        var user = await Arrange.User.WithEmail(value).SeedAsync(Services);

        var payload = new LoginCommand
        {
            Email = variant,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        AssertSession(session);
    }

    [Theory]
    [MemberData(nameof(EquivalentUsernames))]
    public async Task Login_WithValidUsername_ShouldReturnSession(string value, string variant)
    {
        var user = await Arrange.User.WithUsername(value).SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = variant,
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        AssertSession(session);
    }

    private static void AssertSession(LoginResponse? session)
    {
        Assert.NotNull(session);
        Assert.NotEmpty(session.AccessToken);
        Assert.NotEmpty(session.RefreshToken);
    }
}