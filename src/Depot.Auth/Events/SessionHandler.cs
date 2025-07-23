namespace Depot.Auth.Events;

using System.Reactive;
using System.Reactive.Threading.Tasks;
using Domain.Events;
using Mestra.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

public class SessionHandler :
    IMessageHandler<SessionCreatedEvent>,
    IMessageHandler<SessionRefreshedEvent>,
    IMessageHandler<SessionRevokedEvent>
{
    private readonly IDistributedCache _cache;

    public SessionHandler(IDistributedCache cache)
    {
        _cache = cache;
    }

    public IObservable<Unit> Handle(SessionCreatedEvent message)
    {
        return _cache.SetAsync(
                message.SessionId,
                null!,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = message.ExpiresAt
                })
            .ToObservable();
    }

    public IObservable<Unit> Handle(SessionRefreshedEvent message)
    {
        return _cache
            .SetAsync(message.SessionId,
                null!,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = message.ExpiresAt
                })
            .ToObservable();
    }

    public IObservable<Unit> Handle(SessionRevokedEvent message)
    {
        return _cache.RemoveAsync(message.SessionId).ToObservable();
    }
}