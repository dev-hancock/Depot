using Depot.Repository.Endpoints.Meta;

namespace Depot.Repository.Endpoints.Artifacts;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapArtifactsEndpoints(this IEndpointRouteBuilder routes)
    {
        var api = routes
            .MapGroup("artifacts")
            .WithTags("artifacts")
            .RequireAuthorization();

        api.MapPost("{repository}/{*path}", UploadEndpoint.Handle)
            .Accepts<IFormFileCollection>("multipart/form-data")
            .WithName("upload");

        api.MapGet("{repository}/{*path}", DownloadEndpoint.Handle);

        api.MapDelete("{repository}/{*path}", DeleteEndpoint.Handle);

        // TODO: 
        api.MapGet("/{id}/meta", MetaEndpoint.Handle);

        api.MapGet("", QueryEndpoint.Handle);

        return routes;
    }
}