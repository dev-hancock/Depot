using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Depot.Auth.Services;

public interface ISecureRandom
{
    string Next(int length);
}

public class SecureRandom : ISecureRandom
{
    public string Next(int length)
    {
        return Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(length));
    }
}