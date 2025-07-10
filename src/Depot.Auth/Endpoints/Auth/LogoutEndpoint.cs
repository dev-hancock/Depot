namespace Depot.Auth.Endpoints.Auth;

using System.Reactive.Threading.Tasks;
using Extensions;
using Features.Auth.Logout;
using Mestra.Abstractions;

public static class LogoutEndpoint
{
    public static Task<IResult> Handle(LogoutCommand request, IMediator mediator, HttpContext context)
    {
        return mediator.Send(request).ToTask(context.RequestAborted).ToOkAsync();
    }
}