namespace Depot.Auth.Domain.Auth;

public sealed record RefreshToken
{
    internal RefreshToken(string value, DateTimeOffset expiresAt)
    {
        Value = value;
        ExpiresAt = expiresAt;
    }

    public DateTimeOffset ExpiresAt { get; private init; }

    public string Value { get; } = null!;

    public static RefreshToken Create(string secret, DateTimeOffset expiresAt)
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