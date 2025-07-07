namespace Depot.Auth.Endpoints.Users;

using System.Reactive.Threading.Tasks;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Common.Models;
using Extensions;
using Handlers;
using Handlers.Users;
using Mestra.Abstractions;

public static class MeEndpoint
{
    public async static Task<IResult> Handle(IMediator mediator, HttpContext context)
    {
        var id = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (id is null)
        {
            return Results.Unauthorized();
        }

        var result = await mediator
            .Send(new MeHandler.Request(Guid.Parse(id)))
            .ToTask(context.RequestAborted);

        return result
            .Match(
                ok => Results.Ok(new Response
                {
                    User = new User
                    {
                        Username = ok.Username
                    }
                }),
                errors => errors.ToResult());
    }

    private sealed class Response
    {
        [JsonPropertyName("user")]
        public User User { get; init; } = null!;
    }
}