namespace Depot.Auth.Tests.Features.Auth.Login.Security;

using System.Net;
using System.Net.Http.Json;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Login;
using Microsoft.AspNetCore.Mvc;

public class Failure(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    private const string ValidUsername = "valid";

    private const string ValidEmail = "valid@example.com";

    private const string ValidPassword = "Sup3r$ecret!";

    private const string InvalidUsername = "invalid";

    private const string InvalidEmail = "invalid@example.com";

    private const string InvalidPassword = "Super$ecr3t!";

    public static IEnumerable<object?[]> InvalidVariants =>
    [
        [ValidUsername, null, InvalidPassword],
        [null, ValidEmail, InvalidPassword],

        [InvalidUsername, null, ValidPassword],
        [null, InvalidEmail, ValidPassword],

        [ValidUsername, null, $" {InvalidPassword} "],
        [null, ValidEmail, $" {InvalidPassword} "]
    ];

    [Theory]
    [MemberData(nameof(InvalidVariants))]
    public async Task Login_WithWrongCredentials_ShouldReturnUnauthorized(string? username, string? email, string? password)
    {
        await Arrange.User
            .WithUsername(ValidUsername)
            .WithEmail(ValidEmail)
            .WithPassword(ValidPassword)
            .SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = username,
            Email = email,
            Password = password!
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(content);
    }
}