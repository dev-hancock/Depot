namespace Depot.Repository.Endpoints;

using Microsoft.AspNetCore.Mvc;

public static class DownloadEndpoint
{
    public async static Task<IResult> Handle([FromRoute] string repository, [FromRoute] string path, HttpContext context)
    {
        return Results.NoContent();
    }
}