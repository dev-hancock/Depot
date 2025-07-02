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

    public static Token New(Guid id, User user, TokenType type, string value, DateTime now, TimeSpan lifetime)
    {
        return new Token
        {
            Id = id,
            User = user,
            Type = type,
            Value = value,
            CreatedAt = now,
            ExpiresAt = now.Add(lifetime)
        };
    }
}

//
// public static ErrorOr<Unit> Validate(Token? token, ISecretHasher hasher, TimeProvider time, string secret)
// {
//     if (token is null)
//     {
//         return Errors.TokenInvalid(TokenType.Refresh);
//     }
//
//     if (token.IsRevoked)
//     {
//         return Errors.TokenInvalid(TokenType.Refresh);
//     }
//
//     if (token.IsExpired(time))
//     {
//         return Errors.TokenInvalid(TokenType.Refresh);
//     }
//
//     if (!hasher.Verify(token.Value, secret))
//     {
//         return Errors.TokenInvalid(TokenType.Refresh);
//     }
//
//     return Unit.Default;
// }