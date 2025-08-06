namespace Depot.Auth.Endpoints;

using System.Reactive.Threading.Tasks;
using Extensions;
using Features.Auth.Login;
using Features.Auth.Logout;
using Features.Auth.RefreshToken;
using Features.Auth.Register;
using Mestra.Abstractions;
using Microsoft.AspNetCore.Mvc;

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

    private static Task<IResult> RefreshTokenAsync([FromBody] RefreshTokenCommand request, [FromServices] IMediator mediator,
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