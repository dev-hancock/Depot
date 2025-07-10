namespace Depot.Auth.Endpoints.Organisations;

using System.Reactive.Threading.Tasks;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Extensions;
using Features.Organisations;
using Mestra.Abstractions;

public class CreateOrganisationEndpoint
{
    public async static Task<IResult> Handle(CreateOrganisationRequest request, IMediator mediator, HttpContext context)
    {
        var id = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (id is null)
        {
            return Results.Unauthorized();
        }

        var result = await mediator
            .Send(new CreateOrganisationHandler.Request(
                Guid.Parse(id),
                request.Name))
            .ToTask(context.RequestAborted);

        return result
            .Match(
                ok => Results.Created($"/organisation/{ok.Slug}",
                    new CreateOrganisationResponse
                    {
                        Key = ok.Slug,
                        Name = ok.Name
                    }),
                errors => errors.ToResult());
    }

    public sealed class CreateOrganisationRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }

    public sealed class CreateOrganisationResponse
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }
}