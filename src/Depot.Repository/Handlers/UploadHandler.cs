namespace Depot.Repository.Handlers;

using System.Reactive;
using System.Reactive.Linq;
using Mestra.Abstractions;

public class UploadHandler : IMessageHandler<UploadHandler.Request>
{
    public IObservable<Unit> Handle(Request message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private Task<Unit> Handle(Request message, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public sealed record Request : IRequest;
}