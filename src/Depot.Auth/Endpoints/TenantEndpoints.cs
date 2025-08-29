using System.Reactive.Threading.Tasks;
using Depot.Auth.Extensions;
using Depot.Auth.Features.Users.Me;
using Mestra.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Depot.Auth.Endpoints;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder routes)
    {
        var api = routes.MapGroup("/tenants").WithTags("Tenants");

        api.MapPost("/{tenant_id}/roles", CreateRoleAsync)
            .WithDescription("Create role in tenant");

        api.MapPost("/{tenant_id}/roles/{role_id}/permissions", AddPermissionAsync)
            .WithDescription("Add permission to role");

        api.MapPost("/{tenant_id}/members/{user_id}/roles/{role_id}", AssignRoleAsync)
            .WithDescription("Assign role to user in tenant");


        return api;
    }

    private static Task<IResult> AddPermissionAsync([FromServices] IMediator mediator, HttpContext context)
    {
        return mediator.Send(new MeQuery()).ToTask(context.RequestAborted).ToOkAsync();
    }

    private static Task<IResult> AssignRoleAsync([FromServices] IMediator mediator, HttpContext context)
    {
        return mediator.Send(new MeQuery()).ToTask(context.RequestAborted).ToOkAsync();
    }

    private static Task<IResult> CreateRoleAsync([FromServices] IMediator mediator, HttpContext context)
    {
        return mediator.Send(new MeQuery()).ToTask(context.RequestAborted).ToOkAsync();
    }
}