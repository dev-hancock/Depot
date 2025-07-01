namespace Depot.Auth.Endpoints;

using Services;

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

public static class RefreshEndpoint
{
    public async static Task<IResult> Handle(string token, IUserService users, ITokenService tokens, HttpContext context)
    {
        return Results.Ok();
    }

    public sealed class Response
    {
    }
}

public static class ChangePasswordEndpoint
{
    public async static Task<IResult> Handle(HttpContext context)
    {
        return Results.Ok();
    }

    public sealed class Request
    {
    }
}