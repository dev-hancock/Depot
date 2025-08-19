namespace Depot.Auth.Tests.Features.Auth.Login.Contract;

using System.Net;
using System.Net.Http.Json;
using Data.Extensions;
using Depot.Auth.Features.Auth.Login;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

public class Validation(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    public static TheoryData<LoginPayload> InvalidVariants()
    {
        return
        [
            new LoginPayload(null, Faker.Internet.Email(), null)
            {
                Label = "Email only"
            },
            new LoginPayload(null, Faker.Internet.Email(), "")
            {
                Label = "Email with empty password"
            },
            new LoginPayload(null, "", null)
            {
                Label = "Empty email and password"
            },
            new LoginPayload(null, " ", null)
            {
                Label = "Whitespace email and password"
            },
            new LoginPayload(null, "not-an-email", null)
            {
                Label = "Invalid email format"
            },
            new LoginPayload(null, "not-an-email", Faker.Internet.StrongPassword())
            {
                Label = "Invalid email format with password"
            },

            new LoginPayload(Faker.Internet.UserName(), null, null)
            {
                Label = "Username only"
            },
            new LoginPayload(Faker.Internet.UserName(), null, "")
            {
                Label = "Username with empty password"
            },
            new LoginPayload("", null, "")
            {
                Label = "Empty username and password"
            },
            new LoginPayload(" ", null, "")
            {
                Label = "Whitespace username and password"
            },

            new LoginPayload(null, null, Faker.Internet.StrongPassword())
            {
                Label = "Password only"
            },
            new LoginPayload(Faker.Internet.UserName(), Faker.Internet.Email(), Faker.Internet.StrongPassword())
            {
                Label = "Valid username and email with password"
            },
            new LoginPayload(null, null, null)
            {
                Label = "All fields empty"
            }
        ];
    }

    [Theory]
    [MemberData(nameof(InvalidVariants))]
    public async Task Login_WithInvalidPayload_ShouldReturnBadRequest(LoginPayload value)
    {
        var payload = new LoginCommand
        {
            Username = value.Username,
            Email = value.Email,
            Password = value.Password!
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

    public record LoginPayload(string? Username, string? Email, string? Password)
    {
        public string Label { get; init; } = "";

        public override string ToString()
        {
            return Label;
        }
    }
}