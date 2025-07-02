namespace Depot.Auth.Endpoints;

using Common;
using Mestra.Abstractions;

public static class RefreshEndpoint
{
    public async static Task<IResult> Handle(string refresh, IMediator mediator, HttpContext context)
    {
        return Results.Ok();
    }

    public sealed class Response
    {
        public Session Session { get; private init; }
    }
}