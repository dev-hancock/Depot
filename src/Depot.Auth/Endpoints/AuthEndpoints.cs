using System.Reactive.Threading.Tasks;
using Depot.Auth.Extensions;
using Depot.Auth.Features.Auth.Login;
using Depot.Auth.Features.Auth.Logout;
using Depot.Auth.Features.Auth.Refresh;
using Depot.Auth.Features.Auth.Register;
using Mestra.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Depot.Auth.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var api = routes.MapGroup("auth").WithTags("Auth");

        api.MapPost("/login", LoginAsync)
            .WithDescription("")
            .AllowAnonymous();

        api.MapPost("/logout", LogoutAsync)
            .WithDescription("");

        api.MapPost("/refresh", RefreshTokenAsync)
            .WithDescription("");

        api.MapPost("/register", RegisterAsync)
            .WithDescription("")
            .AllowAnonymous();

        return routes;
    }

    private static Task<IResult> LoginAsync([FromBody] LoginCommand request, [FromServices] IMediator mediator, HttpContext context)
    {
        return mediator.Send(request).ToTask(context.RequestAborted).ToOkAsync();
    }

    private static Task<IResult> LogoutAsync([FromBody] LogoutCommand request, [FromServices] IMediator mediator, HttpContext context)
    {
        return mediator.Send(request).ToTask(context.RequestAborted).ToOkAsync();
    }

    private static Task<IResult> RefreshTokenAsync([FromBody] RefreshCommand request, [FromServices] IMediator mediator,
        HttpContext context)
    {
        return mediator.Send(request).ToTask(context.RequestAborted).ToOkAsync();
    }

    private static Task<IResult> RegisterAsync([FromBody] RegisterCommand request, [FromServices] IMediator mediator,
        HttpContext context)
    {
        return mediator.Send(request).ToTask(context.RequestAborted).ToOkAsync();
    }
}