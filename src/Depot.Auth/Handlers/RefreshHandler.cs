namespace Depot.Auth.Handlers;

using System.Reactive.Linq;
using Domain;
using ErrorOr;
using Mestra.Abstractions;

public class RefreshHandler : IMessageHandler<RefreshHandler.Request, ErrorOr<Session>>
{
    public IObservable<ErrorOr<Session>> Handle(Request message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private Task<ErrorOr<Session>> Handle(Request message, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public sealed record Request(string Token) : IRequest<ErrorOr<Session>>;
}