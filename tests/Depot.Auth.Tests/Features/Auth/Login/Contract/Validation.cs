namespace Depot.Auth.Tests.Features.Auth.Login.Contract;

using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Login;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Setup;

public class Validation
{
    private const string ValidUsername = "username";

    private const string ValidEmail = "user@example.com";

    private const string ValidPassword = "Sup3r$ecret!";

    public static IEnumerable<object?[]> InvalidVariants()
    {
        return
        [
            [null, ValidEmail, null],
            [null, ValidEmail, ""],
            [null, ValidEmail, " "],

            [null, "not-an-email", null],
            [null, "not-an-email", ValidPassword],

            [ValidUsername, null, null],
            [ValidUsername, null, ""],
            [ValidUsername, null, " "],

            ["", null, ValidPassword],
            [" ", null, ValidPassword],
            [null, "", ValidPassword],
            [null, " ", ValidPassword],
            [null, null, ValidPassword],

            ["", null, ""],
            [" ", null, ""],
            [" ", null, " "],
            [null, "", null],
            [null, " ", ""],
            [null, " ", " "],
            ["", "", ""],
            [" ", "", ""],
            ["", " ", ""],
            [" ", " ", ""],

            [ValidUsername, ValidEmail, ValidPassword],
            [ValidEmail, null, ValidPassword],
            [null, ValidUsername, ValidPassword],
            [ValidEmail, null, ValidPassword],

            [ValidUsername, "", ValidPassword],
            ["", ValidEmail, ValidPassword],

            [ValidUsername, " ", ValidPassword],
            [" ", ValidEmail, ValidPassword],

            [ValidUsername, ValidEmail, ""],
            [ValidUsername, ValidEmail, " "],

            [null, null, null]
        ];
    }

    [Test]
    [MethodDataSource(nameof(InvalidVariants))]
    public async Task Login_WithInvalidPayload_ShouldReturnBadRequest(string? username, string? email, string? password)
    {
        var payload = new LoginCommand
        {
            Username = username,
            Email = email,
            Password = password!
        };

        var response = await Requests.Post("api/v1/auth/login", payload).SendAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        var content = await Assert.That(result).IsNotNull();

        await Assert.That(content.Title).IsEqualTo(ReasonPhrases.GetReasonPhrase(400));
        await Assert.That(content.Status).IsEqualTo(400);
        await Assert.That(content.Detail!).IsNotEmpty();

        await Assert.That(content.Extensions["errors"]).IsNotNull();
    }
}