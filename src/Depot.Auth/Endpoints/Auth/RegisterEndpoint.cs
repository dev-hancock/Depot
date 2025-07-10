namespace Depot.Auth.Endpoints.Auth;

using System.Reactive.Threading.Tasks;
using Extensions;
using Features.Auth.Register;
using Mestra.Abstractions;
using Microsoft.AspNetCore.Mvc;

public static class RegisterEndpoint
{
    public static Task<IResult> Handle([FromBody] RegisterCommand request, IMediator mediator, HttpContext context)
    {
        return mediator.Send(request).ToTask(context.RequestAborted).ToOkAsync();
    }
}