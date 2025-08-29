using System.Security.Cryptography;
using Depot.Auth.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Depot.Auth.Services;

public class SecureRandom : ISecureRandom
{
    public string Next(int length)
    {
        return Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(length));
    }
}