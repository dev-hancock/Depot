namespace Depot.Auth.Endpoints;

using System.IdentityModel.Tokens.Jwt;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using Common;
using Extensions;
using Handlers;
using Mestra.Abstractions;

public static class MeEndpoint
{
    public async static Task<IResult> Handle(IMediator mediator, HttpContext context)
    {
        var id = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (id is null)
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(new MeHandler.Request(Guid.Parse(id)));

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