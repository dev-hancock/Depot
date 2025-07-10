namespace Depot.Auth.Domain.Users;

using ErrorOr;
using Interfaces;

public class Password
{
    internal Password(string encoded)
    {
        Encoded = encoded;
    }

    public string Encoded { get; }

    public static Password Parse(string password)
    {
        return new Password(password);
    }

    public static ErrorOr<Password> New(string password, ISecretHasher hasher)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return Error.Validation("");
        }

        return new Password(hasher.Hash(password));
    }

    public bool Verify(string secret, ISecretHasher hasher)
    {
        return hasher.Verify(Encoded, secret);
    }

    public static Password Create(string value)
    {
        return new Password(value);
    }
}