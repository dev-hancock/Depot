namespace Depot.Auth.Endpoints;

using System.ComponentModel.DataAnnotations;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using Extensions;
using Handlers;
using Mestra.Abstractions;

public static class LoginEndpoint
{
    public async static Task<IResult> Handle(LoginRequest request, IMediator mediator, HttpContext context)
    {
        var result = await mediator.Send(new LoginHandler.Request(request.Username, request.Password));

        return result
            .Match(
                token => Results.Ok(new LoginResponse
                {
                    AccessToken = token.Access,
                    RefreshToken = token.Refresh.Combined
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
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = null!;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; init; } = null!;
    }
}