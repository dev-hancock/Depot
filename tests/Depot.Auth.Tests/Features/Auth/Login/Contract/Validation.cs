namespace Depot.Auth.Tests.Features.Auth.Login.Contract;

using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Login;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

public class Validation(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    private const string ValidUsername = "username";

    private const string ValidEmail = "user@example.com";

    private const string ValidPassword = "Sup3r$ecret!";

    public static IEnumerable<object?[]> InvalidVariants =>
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

    [Theory]
    [MemberData(nameof(InvalidVariants))]
    public async Task Login_WithInvalidPayload_ShouldReturnBadRequest(string? username, string? email, string? password)
    {
        var payload = new LoginCommand
        {
            Username = username,
            Email = email,
            Password = password!
        };

        var res = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);

        var content = await res.Content.ReadFromJsonAsync<ProblemDetails>();

        AssertProblem(content);
    }

    private static void AssertProblem(ProblemDetails? content)
    {
        Assert.NotNull(content);
        Assert.Equal(ReasonPhrases.GetReasonPhrase(400), content.Title);
        Assert.Equal(400, content.Status);
        Assert.NotEmpty(content.Detail!);

        Assert.NotNull(content.Extensions["errors"]);
    }
}