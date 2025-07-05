namespace Depot.Repository.Endpoints;

using Microsoft.AspNetCore.Mvc;

public static class DeleteEndpoint
{
    public async static Task<IResult> Handle([FromQuery] string id, HttpContext httpContext)
    {
        return Results.NoContent();
    }
}