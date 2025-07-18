namespace Depot.Auth.Domain.Auth;

public sealed record RefreshToken
{
    private RefreshToken(string value, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        Value = value;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; private init; }

    public DateTime ExpiresAt { get; private init; }

    public string Value { get; } = null!;

    public static implicit operator string(RefreshToken token)
    {
        return token.Value;
    }

    public static RefreshToken Create(string secret, DateTime expiresAt)
    {
        if (secret.Length < 32)
        {
            throw new ArgumentOutOfRangeException(nameof(secret), "Token must be at least 32 characters");
        }

        return new RefreshToken(secret, expiresAt);
    }
}