namespace Depot.Auth.Endpoints;

using System.Reactive.Threading.Tasks;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Extensions;
using Handlers;
using Mestra.Abstractions;

public static class LogoutEndpoint
{
    public async static Task<IResult> Handle(LogoutRequest request, IMediator mediator, HttpContext context)
    {
        var id = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (id is null)
        {
            return Results.Unauthorized();
        }

        var result = await mediator
            .Send(new LogoutHandler.Request(
                Guid.Parse(id),
                request.RefreshToken))
            .ToTask(context.RequestAborted);

        return result.Match(
            _ => Results.NoContent(),
            errors => errors.ToResult());
    }

    public sealed class LogoutRequest
    {
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
    }
}