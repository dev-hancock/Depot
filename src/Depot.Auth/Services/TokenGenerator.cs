namespace Depot.Auth.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class TokenGenerator : ITokenGenerator
{
    private readonly JwtSecurityTokenHandler _handler = new();

    private readonly JwtOptions _options;

    private readonly SigningCredentials _signing;

    public TokenGenerator(IOptions<JwtOptions> options, ECDsaSecurityKey key)
    {
        _options = options.Value;
        _signing = new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256);
    }

    public string CreateAccessToken(User user, DateTime now)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var ur in user.UserRoles)
        {
            if (ur.Role is not null)
            {
                claims.Add(new Claim(ClaimTypes.Role, ur.Role.Name));
            }
        }

        var access = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            now,
            now + _options.AccessTokenLifetime,
            _signing);

        return _handler.WriteToken(access);
    }
}