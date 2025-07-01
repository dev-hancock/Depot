namespace Depot.Auth.Domain;

public enum TokenType
{
    Refresh
}

public class Token
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public string Hash { get; init; } = null!;

    public TokenType Type { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }

    public User User { get; init; } = null!;

    public bool IsRevoked { get; private set; }

    public void Revoke()
    {
        IsRevoked = true;
    }

    public static Token Refresh(User user, RefreshToken token, DateTime now, TimeSpan lifetime)
    {
        return new Token
        {
            Id = token.Id,
            User = user,
            Type = TokenType.Refresh,
            Hash = token.Secret,
            CreatedAt = now,
            ExpiresAt = now.Add(lifetime)
        };
    }

    public bool IsExpired(TimeProvider time)
    {
        return ExpiresAt < time.GetUtcNow();
    }
}