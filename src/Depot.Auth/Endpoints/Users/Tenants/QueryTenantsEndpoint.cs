namespace Depot.Auth.Endpoints.Users.Tenants;

using Mestra.Abstractions;

public class QueryTenantsEndpoint
{
    public async static Task<IResult> Handle(QueryTenantsRequest request, IMediator mediator, HttpContext context)
    {
        throw new NotImplementedException();
    }

    public class QueryTenantsRequest
    {
    }
}