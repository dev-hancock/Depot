namespace Depot.Auth.Endpoints.Users;

using Mestra.Abstractions;

public static class ChangePasswordEndpoint
{
    public async static Task<IResult> Handle(ChangePasswordRequest request, IMediator mediator, HttpContext context)
    {
        return Results.Ok();
    }

    public sealed class ChangePasswordRequest
    {
    }
}