namespace Depot.Auth.Endpoints;

using System.Reactive.Threading.Tasks;
using System.Text.Json.Serialization;
using Common;
using Extensions;
using Handlers;
using Mestra.Abstractions;

public static class RefreshEndpoint
{
    public async static Task<IResult> Handle(RefreshRequest request, IMediator mediator, HttpContext context)
    {
        var result = await mediator
            .Send(new RefreshHandler.Request(request.RefreshToken))
            .ToTask(context.RequestAborted);

        return result.Match(
            ok => Results.Ok(new RefreshResponse
            {
                Session = new Session
                {
                    AccessToken = ok.AccessToken.Value,
                    RefreshToken = ok.RefreshToken.Combined
                }
            }),
            errors => errors.ToResult());
    }

    public sealed class RefreshRequest
    {
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = null!;
    }

    public sealed class RefreshResponse
    {
        [JsonPropertyName("session")]
        public Session Session { get; init; } = null!;
    }
}