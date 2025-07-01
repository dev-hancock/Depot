namespace Depot.Auth.Endpoints;

using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Extensions;
using Services;

public static class MeEndpoint
{
    public async static Task<IResult> Handle(IUserService users, HttpContext context)
    {
        var principal = context.User;

        if (principal.Identity is not { IsAuthenticated: true })
        {
            return Results.Unauthorized();
        }

        var id = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (id is null)
        {
            return Results.Unauthorized();
        }

        var result = await users.GetUserAsync(Guid.Parse(id), context.RequestAborted);

        return result
            .Match(
                user => Results.Ok(new Response
                {
                    Username = user.Username
                }),
                errors => errors.ToResult());
    }

    private sealed class Response
    {
        [JsonPropertyName("username")]
        public string Username { get; init; } = null!;
    }
}