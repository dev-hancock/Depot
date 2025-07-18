namespace Depot.Auth.Domain.Users;

public class Password
{
    private Password(string encoded)
    {
        Encoded = encoded;
    }

    public string Encoded { get; }

    public static implicit operator string(Password password)
    {
        return password.Encoded;
    }

    public static Password Create(string value)
    {
        return new Password(value);
    }
}