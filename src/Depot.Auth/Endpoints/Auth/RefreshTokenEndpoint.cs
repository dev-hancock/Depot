namespace Depot.Auth.Endpoints.Auth;

using System.Reactive.Threading.Tasks;
using Extensions;
using Features.Auth.RefreshToken;
using Mestra.Abstractions;

public static class RefreshTokenEndpoint
{
    public static Task<IResult> Handle(RefreshTokenCommand request, IMediator mediator, HttpContext context)
    {
        return mediator.Send(request).ToTask(context.RequestAborted).ToOkAsync();
    }
}