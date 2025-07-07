namespace Depot.Auth.Endpoints.Auth;

using System.Reactive.Threading.Tasks;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Common.Models;
using Extensions;
using Handlers.Auth;
using Mestra.Abstractions;

public static class RefreshTokenEndpoint
{
    public async static Task<IResult> Handle(RefreshTokenRequest tokenRequest, IMediator mediator, HttpContext context)
    {
        var id = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (id is null)
        {
            return Results.Unauthorized();
        }

        var result = await mediator
            .Send(new RefreshTokenHandler.Request(
                Guid.Parse(id),
                tokenRequest.RefreshToken))
            .ToTask(context.RequestAborted);

        return result.Match(
            ok => Results.Ok(new RefreshTokenResponse
            {
                Session = new Session
                {
                    AccessToken = ok.AccessToken.Value,
                    RefreshToken = ok.RefreshToken.Combined
                }
            }),
            errors => errors.ToResult());
    }

    public sealed class RefreshTokenRequest
    {
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = null!;
    }

    public sealed class RefreshTokenResponse
    {
        [JsonPropertyName("session")]
        public Session Session { get; init; } = null!;
    }
}