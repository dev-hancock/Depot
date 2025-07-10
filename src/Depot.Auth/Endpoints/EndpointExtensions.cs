namespace Depot.Auth.Endpoints;

using Organisations;
using Tenants;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapOrganisationsEndpoints(this IEndpointRouteBuilder routes)
    {
        var api = routes.MapGroup("/organisations").WithTags("organisations");

        api.MapPost("/", CreateOrganisationEndpoint.Handle)
            .WithDescription("Create organisations");

        api.MapPost("/{organisation_id}/tenants", CreateTenantEndpoint.Handle)
            .WithDescription("Create tenant in organisations");

        api.MapPost("/{organisation_id}/invitations", InviteMemberEndpoint.Handle)
            .WithDescription("Invite a user to organisations");

        api.MapDelete("/{organisation_id}/members/{user_id}", RemoveMemberEndpoint.Handle)
            .WithDescription("Remove user from organisations");

        return api;
    }

    public static IEndpointRouteBuilder MapTenantsEndpoints(this IEndpointRouteBuilder routes)
    {
        var api = routes.MapGroup("/tenants").WithTags("tenants");

        api.MapPost("/{tenant_id}/roles", CreateRoleEndpoint.Handle)
            .WithDescription("Create role in tenant");

        api.MapPost("/{tenant_id}/roles/{role_id}/permissions", AddPermissionEndpoint.Handle)
            .WithDescription("Add permission to role");

        api.MapPost("/{tenant_id}/members/{user_id}/roles/{role_id}", AssignRoleEndpoint.Handle)
            .WithDescription("Assign role to user in tenant");

        return api;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder routes)
    {
        var api = routes.MapGroup("api").WithTags("Api").RequireAuthorization();

        api.MapAuthEndpoints();

        // api.MapOrganisationsEndpoints();
        //
        // api.MapTenantsEndpoints();

        api.MapUserEndpoints();

        return api;
    }
}