namespace Depot.Auth.Endpoints.Users;

using Mestra.Abstractions;
using Microsoft.AspNetCore.Mvc;

public class GetOrganisationEndpoint
{
    public async static Task<IResult> Handle([AsParameters] GetOrganisationRequest request, IMediator mediator, HttpContext context)
    {
        throw new NotImplementedException();
    }

    public sealed class GetOrganisationRequest
    {
        [FromRoute(Name = "key")]
        public required string Key { get; set; } = null!;
    }

    public sealed class GetOrganisationResponse
    {
        // TODO:
    }
}