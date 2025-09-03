using System.Reactive;
using System.Reactive.Threading.Tasks;
using Depot.Auth.Domain.Users.Events;
using Depot.Auth.Extensions;
using Mestra.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Depot.Auth.Events;

public static class CacheKeys
{
    public static string Session(Guid id)
    {
        return $"sid:{id}:ver";
    }
}

public class SessionHandler(IDistributedCache cache, IOptions<JwtOptions> options) :
    IMessageHandler<SessionCreatedEvent>,
    IMessageHandler<SessionRefreshedEvent>,
    IMessageHandler<SessionRevokedEvent>
{
    private readonly JwtOptions _options = options.Value;

    private readonly TimeSpan _skew = TimeSpan.FromMinutes(2);

    public IObservable<Unit> Handle(SessionCreatedEvent message)
    {
        return SetSession(message.Id, message.Version).ToObservable();
    }

    public IObservable<Unit> Handle(SessionRefreshedEvent message)
    {
        return SetSession(message.Id, message.Version).ToObservable();
    }

    public IObservable<Unit> Handle(SessionRevokedEvent message)
    {
        return SetSession(message.Id, message.Version).ToObservable();
    }

    private async Task SetSession(Guid id, int version)
    {
        var key = CacheKeys.Session(id);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _options.AccessTokenLifetime + _skew
        };

        await cache.SetAsync(key, BitConverter.GetBytes(version), options);
    }
}