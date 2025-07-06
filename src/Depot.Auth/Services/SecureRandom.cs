namespace Depot.Auth.Services;

using System.Security.Cryptography;
using Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;

public class SecureRandom : ISecureRandom
{
    public string Next(int length)
    {
        return Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(length));
    }
}