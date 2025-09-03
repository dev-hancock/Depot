namespace Depot.Auth.Domain.Auth;

public sealed record AccessToken
{
    private AccessToken(string value, DateTimeOffset expiresAt)
    {
        Value = value;
        ExpiresAt = expiresAt;
    }

    public string Value { get; }

    public DateTimeOffset ExpiresAt { get; set; }

    public static AccessToken Create(string value, DateTimeOffset expiresAt)
    {
        return new AccessToken(value, expiresAt);
    }

    public static implicit operator string(AccessToken token)
    {
        return token.Value;
    }
}