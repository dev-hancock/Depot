namespace Depot.Auth.Endpoints;

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