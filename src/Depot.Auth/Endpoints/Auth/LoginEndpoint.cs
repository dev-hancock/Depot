namespace Depot.Auth.Endpoints.Auth;

using System.Reactive.Threading.Tasks;
using Extensions;
using Features.Auth.Login;
using Mestra.Abstractions;

public static class LoginEndpoint
{
    public static Task<IResult> Handle(LoginCommand request, IMediator mediator, HttpContext context)
    {
        return mediator.Send(request).ToTask(context.RequestAborted).ToOkAsync();
    }
}