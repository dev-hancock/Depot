namespace Depot.Auth.Domain.Auth;

using Common;
using Users;

public readonly record struct SessionId(Guid Value);

public readonly record struct UserId(Guid Value);

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

    public DateTime ExpiresAt => RefreshToken.ExpiresAt;

    public bool IsRevoked { get; private set; }

    public User User { get; private init; } = null!;

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

    public void Revoke()
    {
        IsRevoked = true;
    }

    public void Refresh(RefreshToken token)
    {
        RefreshToken = token;
    }
}