using Mestra.Abstractions;

namespace Depot.Auth.Endpoints.Organisations;

public class InviteMemberEndpoint
{
    public static async Task<IResult> Handle(InviteMemberRequest request, IMediator mediator, HttpContext context)
    {
        throw new NotImplementedException();
    }

    public class InviteMemberRequest { }
}