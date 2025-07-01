namespace Depot.Auth.Domain;

using ErrorOr;

public sealed record RefreshToken
{
    private const string Split = ":";

    private RefreshToken()
    {
    }

    public Guid Id { get; private init; }

    public string Token => $"{Id}{Split}{Secret}";

    public string Secret { get; private init; } = null!;

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
            Secret = parts[1]
        };
    }

    public static RefreshToken New(ISecureRandom random, int length = 32)
    {
        if (length < 32)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Token must be at least 32 characters");
        }

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Secret = random.Next(length)
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

        if (!hasher.Verify(token.Hash, secret))
        {
            return Errors.TokenInvalid(TokenType.Refresh);
        }

        return Unit.Default;
    }
}