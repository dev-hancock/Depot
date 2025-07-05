namespace Depot.Repository.Endpoints;

using Microsoft.AspNetCore.Mvc;

public static class MetaEndpoint
{
    public async static Task<IResult> Handle([FromRoute] string id, HttpContext httpContext)
    {
        return Results.NoContent();
    }
}