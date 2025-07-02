namespace Depot.Auth.Domain;

public class SecurePassword
{
    private SecurePassword(string encoded)
    {
        Encoded = encoded;
    }

    public string Encoded { get; }

    public static SecurePassword Parse(string password)
    {
        return new SecurePassword(password);
    }

    public static SecurePassword New(string password, ISecretHasher hasher)
    {
        return new SecurePassword(hasher.Hash(password));
    }

    public bool Verify(string secret, ISecretHasher hasher)
    {
        return hasher.Verify(Encoded, secret);
    }
}