namespace Depot.Auth.Domain;

using System.Reactive;
using ErrorOr;

public sealed record RefreshToken : Token
{
    private const string Split = ":";

    private RefreshToken()
    {
    }

    public string Combined => $"{Id}{Split}{Value}";

    public static ErrorOr<RefreshToken> Parse(string token)
    {
        var parts = token.Split(Split);

        if (parts.Length != 2)
        {
            return Errors.TokenInvalid(TokenType.Refresh);
        }

        if (!Guid.TryParse(parts[0], out var id))
        {
            return Errors.TokenInvalid(TokenType.Refresh);
        }

        return new RefreshToken
        {
            Id = id,
            Value = parts[1]
        };
    }

    public static RefreshToken New(User user, ISecureRandom random, ISecretHasher hasher, DateTime now, TimeSpan lifetime,
        int length = 32)
    {
        if (length < 32)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Token must be at least 32 characters");
        }

        var secret = random.Next(length);

        return new RefreshToken
        {
            User = user,
            Type = TokenType.Refresh,
            Value = hasher.Hash(secret),
            CreatedAt = now,
            ExpiresAt = now.Add(lifetime)
        };
    }

    public static ErrorOr<Unit> Validate(Token? token, ISecretHasher hasher, TimeProvider time, string secret)
    {
        if (token is null)
        {
            return Errors.TokenInvalid(TokenType.Refresh);
        }

        if (token.IsRevoked)
        {
            return Errors.TokenInvalid(TokenType.Refresh);
        }

        if (token.IsExpired(time))
        {
            return Errors.TokenInvalid(TokenType.Refresh);
        }

        if (!hasher.Verify(token.Value, secret))
        {
            return Errors.TokenInvalid(TokenType.Refresh);
        }

        return Unit.Default;
    }
}