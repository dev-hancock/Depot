namespace Depot.Repository.Endpoints.Artifacts;

using Microsoft.AspNetCore.Mvc;

public static class QueryEndpoint
{
    public async static Task<IResult> Handle(HttpContext httpContext)
    {
        return Results.NoContent();
    }

    public sealed class Request
    {
        [FromQuery]
        public string? Id { get; set; }
    }
}