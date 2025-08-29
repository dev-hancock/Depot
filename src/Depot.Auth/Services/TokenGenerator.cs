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

    public Token GenerateAccessToken(Guid sub, Guid jti, string[] roles, DateTime now)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, sub.ToString()), new(JwtRegisteredClaimNames.Jti, jti.ToString())
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
            now,
            expires,
            _signing);

        var token = _handler.WriteToken(access);

        return new Token(token, expires);
    }

    public Token GenerateRefreshToken(DateTime now)
    {
        var expires = now + _options.RefreshTokenLifetime;

        var bytes = RandomNumberGenerator.GetBytes(32);

        var encoded = Base64UrlEncoder.Encode(bytes);

        return new Token(encoded, expires);
    }
}