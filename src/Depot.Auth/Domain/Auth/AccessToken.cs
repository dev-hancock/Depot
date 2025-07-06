namespace Depot.Auth.Domain.Auth;

public sealed record AccessToken
{
    private AccessToken(string value)
    {
        Value = value;
    }

    public string Value { get; private set; }

    public static AccessToken New(string token)
    {
        return new AccessToken(token);
    }
}