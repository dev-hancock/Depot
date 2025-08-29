using Microsoft.AspNetCore.Mvc;

namespace Depot.Repository.Endpoints.Meta;

public static class MetaEndpoint
{
    public static async Task<IResult> Handle([FromRoute] string id, HttpContext httpContext)
    {
        return Results.NoContent();
    }
}