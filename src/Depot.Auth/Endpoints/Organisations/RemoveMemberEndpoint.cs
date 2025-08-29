using Mestra.Abstractions;

namespace Depot.Auth.Endpoints.Organisations;

public class RemoveMemberEndpoint
{
    public static async Task<IResult> Handle(RemoveMemberRequest request, IMediator mediator, HttpContext context)
    {
        throw new NotImplementedException();
    }

    public class RemoveMemberRequest { }
}