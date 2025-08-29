using System.Reactive;
using System.Reactive.Linq;
using Mestra.Abstractions;

namespace Depot.Repository.Handlers;

public class MetaDataHandler : IMessageHandler<MetaDataHandler.Request>
{
    public IObservable<Unit> Handle(Request message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private Task<Unit> Handle(Request message, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public sealed record Request(string Id) : IRequest;
}