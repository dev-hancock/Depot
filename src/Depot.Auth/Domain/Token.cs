namespace Depot.Auth.Domain;

public enum TokenType
{
    Refresh
}

public record Token
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public string Value { get; init; } = null!;

    public TokenType Type { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }

    public User User { get; init; } = null!;

    public bool IsRevoked { get; private set; }

    public void Revoke()
    {
        IsRevoked = true;
    }

    public bool IsExpired(TimeProvider time)
    {
        return ExpiresAt < time.GetUtcNow();
    }
}