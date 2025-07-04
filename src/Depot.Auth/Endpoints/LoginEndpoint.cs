namespace Depot.Auth.Endpoints;

using System.ComponentModel.DataAnnotations;
using System.Reactive.Threading.Tasks;
using System.Text.Json.Serialization;
using Common;
using Extensions;
using Handlers;
using Mestra.Abstractions;

public static class LoginEndpoint
{
    public async static Task<IResult> Handle(LoginRequest request, IMediator mediator, HttpContext context)
    {
        var result = await mediator
            .Send(new LoginHandler.Request(
                request.Username,
                request.Password))
            .ToTask(context.RequestAborted);

        return result
            .Match(
                ok => Results.Ok(new LoginResponse
                {
                    Session = new Session
                    {
                        AccessToken = ok.AccessToken.Value,
                        RefreshToken = ok.RefreshToken.Combined
                    }
                }),
                errors => errors.ToResult());
    }

    public sealed class LoginRequest
    {
        [Required]
        [JsonPropertyName("username")]
        public string Username { get; init; } = null!;

        [Required]
        [JsonPropertyName("password")]
        public string Password { get; init; } = null!;
    }

    public sealed class LoginResponse
    {
        [JsonPropertyName("session")]
        public Session Session { get; init; } = null!;
    }
}