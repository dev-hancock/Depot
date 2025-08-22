namespace Depot.Auth.Tests.Features.Auth.Login.Security;

using System.Net;
using System.Net.Http.Json;
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
        var user = Fixture.Arrange.User
            .WithUsername(ValidUsername)
            .WithEmail(ValidEmail)
            .WithPassword(ValidPassword)
            .Build();

        await Fixture.Database.SeedAsync(user);

        var payload = new LoginCommand
        {
            Username = username,
            Email = email,
            Password = password!
        };

        var result = await Fixture.Client.Post("api/v1/auth/login", payload).SendAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(content);
    }
}