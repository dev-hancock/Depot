namespace Depot.Auth.Endpoints;

using Microsoft.IdentityModel.JsonWebTokens;
using Services;

public static class LogoutEndpoint
{
    public async static Task<IResult> Handle(IUserService users, ITokenService tokens, HttpContext context)
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

        await tokens.RevokeTokenAsync(Guid.Parse(id), context.RequestAborted);

        return Results.NoContent();
    }
}