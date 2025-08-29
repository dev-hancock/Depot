using Microsoft.AspNetCore.Mvc;

namespace Depot.Repository.Endpoints.Artifacts;

public static class QueryEndpoint
{
    public static async Task<IResult> Handle(HttpContext httpContext)
    {
        return Results.NoContent();
    }

    public sealed class Request
    {
        [FromQuery]
        public string? Id { get; set; }
    }
}