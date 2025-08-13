namespace Depot.Auth.Domain.Auth;

public sealed record AccessToken
{
    private AccessToken(string value, DateTime expiresAt)
    {
        Value = value;
        ExpiresAt = expiresAt;
    }

    public string Value { get; }

    public DateTime ExpiresAt { get; set; }

    public static AccessToken Create(string value, DateTime expiresAt)
    {
        return new AccessToken(value, expiresAt);
    }

    public static implicit operator string(AccessToken token)
    {
        return token.Value;
    }
}