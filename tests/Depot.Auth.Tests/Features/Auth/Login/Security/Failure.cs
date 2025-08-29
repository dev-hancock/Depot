using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Login;
using Depot.Auth.Tests.Setup;
using Microsoft.AspNetCore.Mvc;

namespace Depot.Auth.Tests.Features.Auth.Login.Security;

public class Failure
{
    private const string ValidUsername = "valid";

    private const string ValidEmail = "valid@example.com";

    private const string ValidPassword = "Sup3r$ecret!";

    private const string InvalidUsername = "invalid";

    private const string InvalidEmail = "invalid@example.com";

    private const string InvalidPassword = "Super$ecr3t!";

    public static IEnumerable<object?[]> InvalidVariants()
    {
        return
        [
            [ValidUsername, null, InvalidPassword],
            [null, ValidEmail, InvalidPassword],

            [InvalidUsername, null, ValidPassword],
            [null, InvalidEmail, ValidPassword],

            [ValidUsername, null, $" {InvalidPassword} "],
            [null, ValidEmail, $" {InvalidPassword} "]
        ];
    }

    [Test]
    [MethodDataSource(nameof(InvalidVariants))]
    public async Task Login_WithWrongCredentials_ShouldReturnUnauthorized(string? username, string? email, string? password)
    {
        var user = Arrange.User
            .WithUsername(ValidUsername)
            .WithEmail(ValidEmail)
            .WithPassword(ValidPassword)
            .Build();

        await Database.SeedAsync(user);

        var payload = new LoginCommand
        {
            Username = username, Email = email, Password = password!
        };

        var response = await Requests.Post("api/v1/auth/login", payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);

        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        _ = await Assert.That(result).IsNotNull();
    }
}