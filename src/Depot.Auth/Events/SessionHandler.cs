using System.Reactive;
using System.Reactive.Threading.Tasks;
using Depot.Auth.Domain.Users.Events;
using Mestra.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace Depot.Auth.Events;

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
                message.SessionId.Value.ToString(),
                [0x1],
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = message.ExpiresAt
                })
            .ToObservable();
    }

    public IObservable<Unit> Handle(SessionRefreshedEvent message)
    {
        return _cache
            .SetAsync(
                message.SessionId.Value.ToString(),
                [0x1],
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = message.ExpiresAt
                })
            .ToObservable();
    }

    public IObservable<Unit> Handle(SessionRevokedEvent message)
    {
        return _cache
            .RemoveAsync(
                message.SessionId.Value.ToString())
            .ToObservable();
    }
}