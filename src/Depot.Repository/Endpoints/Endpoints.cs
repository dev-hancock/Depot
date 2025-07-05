namespace Depot.Repository.Endpoints;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder routes)
    {
        var api = routes
            .MapGroup("artifacts")
            .WithTags("artifacts")
            .RequireAuthorization();

        api.MapPost("/", UploadEndpoint.Handle);

        api.MapGet("/{id}", DownloadEndpoint.Handle);

        api.MapGet("/{id}/meta", MetaDataEndpoint.Handle);

        api.MapGet("/", QueryEndpoint.Handle);

        api.MapDelete("/{id}", DeleteEndpoint.Handle);

        return routes;
    }
}