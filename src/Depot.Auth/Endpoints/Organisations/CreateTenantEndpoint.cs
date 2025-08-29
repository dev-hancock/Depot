using Mestra.Abstractions;

namespace Depot.Auth.Endpoints.Organisations;

public class CreateTenantEndpoint
{
    public static async Task<IResult> Handle(CreateTenantRequest request, IMediator mediator, HttpContext context)
    {
        throw new NotImplementedException();
    }

    public class CreateTenantRequest { }
}