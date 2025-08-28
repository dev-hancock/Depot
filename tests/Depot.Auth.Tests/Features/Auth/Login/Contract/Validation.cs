namespace Depot.Auth.Tests.Features.Auth.Login.Contract;

using System.Net;
using System.Net.Http.Json;
using Depot.Auth.Features.Auth.Login;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Setup;

public class Validation
{
    public static readonly string Password = "Super$ecr3t!";

    private static readonly string Hash = Unique.Hash();

    public static readonly string Username = Unique.Username(Hash);

    public static readonly string Email = Unique.Email(Hash);

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

    public static IEnumerable<object?[]> InvalidVariants()
    {
        return
        [
            [null, Email, null],
            [null, Email, ""],
            [null, Email, " "],

            [null, "not-an-email", null],
            [null, "not-an-email", Username],

            [Username, null, null],
            [Username, null, ""],
            [Username, null, " "],

            ["", null, Username],
            [" ", null, Username],
            [null, "", Username],
            [null, " ", Username],
            [null, null, Username],

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

            [Username, Email, Password],
            [Email, null, Password],
            [null, Username, Password],
            [Email, null, Password],

            [Username, "", Password],
            ["", Email, Password],

            [Username, " ", Password],
            [" ", Email, Password],

            [Username, Email, ""],
            [Username, Email, " "],

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