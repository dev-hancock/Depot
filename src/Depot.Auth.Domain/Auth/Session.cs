using Depot.Auth.Domain.Common;
using Depot.Auth.Domain.Users;

namespace Depot.Auth.Domain.Auth;

public readonly record struct SessionId(Guid Value)
{
    public static implicit operator Guid(SessionId id)
    {
        return id.Value;
    }
}

public readonly record struct UserId(Guid Value)
{
    public static implicit operator Guid(UserId id)
    {
        return id.Value;
    }
}

public class Session : Entity
{
    // EF Core
    private Session() { }

    internal Session(SessionId id, UserId userId, RefreshToken token, bool isRevoked)
    {
        Id = id;
        UserId = userId;
        RefreshToken = token;
        IsRevoked = isRevoked;
    }

    public SessionId Id { get; private init; }

    public UserId UserId { get; private init; }

    public RefreshToken RefreshToken { get; private set; } = null!;

    public DateTimeOffset ExpiresAt => RefreshToken.ExpiresAt;

    public bool IsRevoked { get; private set; }

    public User User { get; private init; } = null!;

    public int Version { get; private set; } = 1;

    public static Session Create(UserId userId, RefreshToken token)
    {
        return new Session(
            new SessionId(Guid.NewGuid()),
            userId,
            token,
            false
        );
    }

    public bool IsExpired(DateTime now)
    {
        return ExpiresAt < now;
    }

    public bool IsValid(DateTime now)
    {
        return !IsExpired(now) && !IsRevoked;
    }

    public void Refresh(RefreshToken token)
    {
        RefreshToken = token;
        Version++;
    }

    public void Revoke()
    {
        IsRevoked = true;
        Version++;
    }
}