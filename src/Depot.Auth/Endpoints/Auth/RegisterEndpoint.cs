namespace Depot.Auth.Endpoints.Auth;

using System.Reactive.Threading.Tasks;
using System.Text.Json.Serialization;
using Common.Models;
using Extensions;
using Handlers;
using Handlers.Auth;
using Mestra.Abstractions;
using Microsoft.AspNetCore.Mvc;

public static class RegisterEndpoint
{
    public async static Task<IResult> Handle([FromBody] RegisterRequest request, IMediator mediator, HttpContext context)
    {
        var result = await mediator
            .Send(new RegisterHandler.Request(
                request.Username,
                request.Email,
                request.Password,
                request.Roles))
            .ToTask(context.RequestAborted);

        return result.Match(
            ok => Results.Created("me",
                new RegisterResponse
                {
                    User = new User
                    {
                        Username = ok.User.Username
                    },
                    Session = new Session
                    {
                        AccessToken = ok.Session.AccessToken.Value,
                        RefreshToken = ok.Session.RefreshToken.Combined
                    }
                }),
            errors => errors.ToResult());
    }

    public sealed class RegisterRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; init; } = null!;

        [JsonPropertyName("email")]
        public string Email { get; init; } = null!;

        [JsonPropertyName("password")]
        public string Password { get; init; } = null!;

        [JsonPropertyName("roles")]
        public string[] Roles { get; init; } = [];
    }

    public sealed class RegisterResponse
    {
        [JsonPropertyName("user")]
        public User User { get; init; } = null!;

        [JsonPropertyName("session")]
        public Session? Session { get; init; }
    }
}