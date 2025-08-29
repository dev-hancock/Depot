using Microsoft.AspNetCore.Mvc;

namespace Depot.Repository.Endpoints.Artifacts;

public static class DeleteEndpoint
{
    public static async Task<IResult> Handle([FromRoute] string repository, [FromRoute] string path, HttpContext context)
    {
        return Results.NoContent();
    }
}