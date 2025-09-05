using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Depot.Auth.Services;

public class TokenGenerator(string issuer, string audience, SecurityKey key)
{
    private readonly JwtSecurityTokenHandler _handler = new();

    private readonly SigningCredentials _signing = new(key, SecurityAlgorithms.EcdsaSha256);

    public string GenerateAccessToken(Guid sub, Guid sid, string[] roles, DateTimeOffset now, DateTimeOffset expiry, int version = 0)
    {
        var jti = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new("sub", sub.ToString()),
            new("jti", jti.ToString()),
            new("sid", sid.ToString()),
            new("ver", version.ToString()),
            new("iat", now.ToUnixTimeSeconds().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var access = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            now.UtcDateTime,
            expiry.UtcDateTime,
            _signing);

        var token = _handler.WriteToken(access);

        return token;
    }

    public string GenerateRefreshToken(int length = 64)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        var token = new char[length];

        var bytes = RandomNumberGenerator.GetBytes(length);

        for (var i = 0; i < length; i++)
        {
            token[i] = chars[bytes[i] % chars.Length];
        }

        return new string(token);
    }
}