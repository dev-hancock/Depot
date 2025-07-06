namespace Depot.Auth.Domain.Interfaces;

public interface ISecretHasher
{
    bool Verify(string encoded, string secret);

    string Hash(string secret);
}