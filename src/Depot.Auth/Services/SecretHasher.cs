using Isopoh.Cryptography.Argon2;

namespace Depot.Auth.Services;

public interface ISecretHasher
{
    string Hash(string secret);

    bool Verify(string encoded, string secret);
}

public class SecretHasher : ISecretHasher
{
    public string Hash(string secret)
    {
        return Argon2.Hash(secret);
    }

    public bool Verify(string encoded, string secret)
    {
        return Argon2.Verify(encoded, secret);
    }
}