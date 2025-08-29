namespace Depot.Auth.Domain.Interfaces;

public interface ISecretHasher
{
    string Hash(string secret);
    bool Verify(string encoded, string secret);
}