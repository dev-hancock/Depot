namespace Depot.Repository.Endpoints;

public static class UploadEndpoint
{
    public async static Task<IResult> Handle(HttpContext httpContext)
    {
        return Results.NoContent();
    }
}