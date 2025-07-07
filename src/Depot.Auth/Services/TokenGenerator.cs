namespace Depot.Auth.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

    public AccessToken CreateAccessToken(User user, DateTime now)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // TODO: 
        // foreach (var ur in user.UserRoles)
        // {
        //     if (ur.Role is not null)
        //     {
        //         claims.Add(new Claim(ClaimTypes.Role, ur.Role.Name));
        //     }
        // }

        var access = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            now,
            now + _options.AccessTokenLifetime,
            _signing);

        return AccessToken.New(_handler.WriteToken(access));
    }
}