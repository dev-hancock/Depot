namespace Depot.Auth.Endpoints.Users;

using System.Reactive.Threading.Tasks;
using Extensions;
using Features.Users.Me;
using Mestra.Abstractions;

public static class MeEndpoint
{
    public static Task<IResult> Handle(IMediator mediator, HttpContext context)
    {
        return mediator.Send(new MeQuery()).ToTask(context.RequestAborted).ToOkAsync();
    }
}