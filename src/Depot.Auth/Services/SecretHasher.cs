namespace Depot.Auth.Services;

using Domain.Interfaces;
using Isopoh.Cryptography.Argon2;

public class SecretHasher : ISecretHasher
{
    public bool Verify(string encoded, string secret)
    {
        return Argon2.Verify(encoded, secret);
    }

    public string Hash(string secret)
    {
        return Argon2.Hash(secret);
    }
}