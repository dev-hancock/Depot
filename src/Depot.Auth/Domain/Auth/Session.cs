namespace Depot.Auth.Domain.Auth;

using Common;

public record SessionId : Identity<SessionId>;

public record UserId : Identity<UserId>;

public class Session
{
    private Session(UserId userId)
    {
        Id = SessionId.Next();
        UserId = userId;
    }

    public SessionId Id { get; }

    public UserId UserId { get; }

    public RefreshToken RefreshToken { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public static Session Create(UserId userId)
    {
        return new Session(userId);
    }

    public bool IsExpired(DateTime now)
    {
        return ExpiresAt < now;
    }

    public bool IsValid()
    {
        return true;
    }

    public void Refresh(RefreshToken token)
    {
        RefreshToken = token;
        ExpiresAt = token.ExpiresAt;
    }
}