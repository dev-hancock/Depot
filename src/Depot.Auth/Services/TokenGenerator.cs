namespace Depot.Auth.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Domain.Auth;
using Domain.Interfaces;
using Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Options;

public class TokenGenerator : ITokenGenerator
{
    private readonly JwtSecurityTokenHandler _handler = new();

    private readonly JwtOptions _options;

    private readonly SigningCredentials _signing;

    public TokenGenerator(IOptions<JwtOptions> options, ISecurityKeyProvider key)
    {
        _options = options.Value;
        _signing = new SigningCredentials(key.GetSecurityKey(_options.KeyPath), SecurityAlgorithms.EcdsaSha256);
    }

    public RefreshToken GenerateRefreshToken(DateTime now)
    {
        var expires = now + _options.RefreshTokenLifetime;

        var bytes = RandomNumberGenerator.GetBytes(32);

        var encoded = Base64UrlEncoder.Encode(bytes);

        return RefreshToken.Create(encoded, expires);
    }

    public AccessToken GenerateAccessToken(User user, SessionId session, DateTime now)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Jti, session.Value.ToString())
        };

        // TODO: 
        // foreach (var ur in user.UserRoles)
        // {
        //     if (ur.Role is not null)
        //     {
        //         claims.Add(new Claim(ClaimTypes.Role, ur.Role.Name));
        //     }
        // }

        var expires = now + _options.AccessTokenLifetime;

        var access = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            now,
            expires,
            _signing);

        var token = _handler.WriteToken(access);

        return AccessToken.Create(token, expires);
    }
}