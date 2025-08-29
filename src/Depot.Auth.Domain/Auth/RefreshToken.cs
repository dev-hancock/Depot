namespace Depot.Auth.Domain.Auth;

public sealed record RefreshToken
{
    internal RefreshToken(string value, DateTime expiresAt)
    {
        Value = value;
        ExpiresAt = expiresAt;
    }

    public DateTime ExpiresAt { get; private init; }

    public string Value { get; } = null!;

    public static RefreshToken Create(string secret, DateTime expiresAt)
    {
        if (secret.Length < 32)
        {
            throw new ArgumentOutOfRangeException(nameof(secret), "Token must be at least 32 characters");
        }

        return new RefreshToken(secret, expiresAt);
    }

    public static implicit operator string(RefreshToken token)
    {
        return token.Value;
    }
}