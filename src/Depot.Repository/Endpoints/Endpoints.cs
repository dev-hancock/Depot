using Depot.Repository.Endpoints.Artifacts;
using Depot.Repository.Endpoints.Meta;

namespace Depot.Repository.Endpoints;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder routes)
    {
        return routes
            .MapGroup("api")
            .MapArtifactsEndpoints();

        var api = routes
            ;

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