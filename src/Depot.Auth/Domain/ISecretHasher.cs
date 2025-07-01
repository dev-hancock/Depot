namespace Depot.Auth.Domain;

public interface ISecretHasher
{
    bool Verify(string encoded, string secret);

    string Hash(string secret);
}