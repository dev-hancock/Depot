namespace Depot.Auth.Endpoints.Users;

using Mestra.Abstractions;

public class SetActiveTenantEndpoint
{
    public async static Task<IResult> Handle(SetActiveTenantRequest request, IMediator mediator, HttpContext context)
    {
        throw new NotImplementedException();
    }

    public class SetActiveTenantRequest
    {
    }
}