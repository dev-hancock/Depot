namespace Depot.Auth.Domain;

public class Session
{
    private Session()
    {
    }

    public AccessToken AccessToken { get; private init; }

    public RefreshToken RefreshToken { get; private init; }

    public DateTime ExpiresAt { get; private init; }

    public static Session New(AccessToken access, RefreshToken refresh, DateTime expires)
    {
        return new Session
        {
            AccessToken = access,
            RefreshToken = refresh,
            ExpiresAt = expires
        };
    }
}