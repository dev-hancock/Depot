using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Depot.Auth.Domain.Interfaces;
using Depot.Auth.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Depot.Auth.Services;

public class TokenGenerator : ITokenGenerator
{
    private readonly JwtSecurityTokenHandler _handler = new();

    private readonly JwtOptions _options;

    private readonly SigningCredentials _signing;

    public TokenGenerator(IOptions<JwtOptions> options, SecurityKey key)
    {
        _options = options.Value;
        _signing = new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256);
    }

    public Token GenerateAccessToken(Guid sub, Guid sid, string[] roles, DateTimeOffset now, int version = 0)
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

        var expires = now + _options.AccessTokenLifetime;

        var access = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            now.UtcDateTime,
            expires.UtcDateTime,
            _signing);

        var token = _handler.WriteToken(access);

        return new Token(token, expires.UtcDateTime);
    }

    public Token GenerateRefreshToken(DateTime now, int length = 64)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        var token = new char[length];

        var bytes = RandomNumberGenerator.GetBytes(length);

        for (var i = 0; i < length; i++)
        {
            token[i] = chars[bytes[i] % chars.Length];
        }

        var expires = now + _options.RefreshTokenLifetime;

        return new Token(new string(token), expires);
    }
}