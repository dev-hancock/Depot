namespace Depot.Auth.Domain;

using System.Reactive;
using ErrorOr;

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

    public static Token New(Guid id, User user, TokenType type, string secret, DateTime now, TimeSpan lifetime, ISecretHasher hasher)
    {
        return new Token
        {
            Id = id,
            User = user,
            Type = type,
            Value = hasher.Hash(secret),
            CreatedAt = now,
            ExpiresAt = now.Add(lifetime)
        };
    }

    public ErrorOr<Unit> Verify(string secret, ISecretHasher hasher, TimeProvider time)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            return Errors.TokenInvalid(Type);
        }

        if (IsRevoked)
        {
            return Errors.TokenInvalid(Type);
        }

        if (IsExpired(time))
        {
            return Errors.TokenInvalid(Type);
        }

        if (!hasher.Verify(Value, secret))
        {
            return Errors.TokenInvalid(Type);
        }

        return Unit.Default;
    }
}