namespace Depot.Auth.Endpoints;

using Mestra.Abstractions;
using Microsoft.IdentityModel.JsonWebTokens;

public static class LogoutEndpoint
{
    public async static Task<IResult> Handle(IMediator mediator, HttpContext context)
    {
        var id = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (id is null)
        {
            return Results.Unauthorized();
        }


        return Results.NoContent();
    }
}