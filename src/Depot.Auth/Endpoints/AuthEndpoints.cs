namespace Depot.Auth.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder routes)
    {
        var api = routes
            .MapGroup("auth")
            .WithTags("auth")
            .RequireAuthorization();

        api.MapPost("/login", LoginEndpoint.Handle)
            .AllowAnonymous();

        api.MapGet("/me", MeEndpoint.Handle);

        api.MapPost("/logout", LogoutEndpoint.Handle);

        api.MapPost("/register", RegisterEndpoint.Handle)
            .AllowAnonymous();

        api.MapPost("/refresh", RefreshEndpoint.Handle);

        api.MapPost("change-password", ChangePasswordEndpoint.Handle);


        return routes;
    }
}