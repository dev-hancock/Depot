using System.Security.Claims;
using Depot.Auth.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace Depot.Auth.Tests.Setup;

public class TokenBuilder(JwtOptions options, SecurityKey key)
{
    private readonly List<Claim> _claims = new();

    private readonly JwtSecurityTokenHandler _handler = new();

    private string _audience = options.Audience;

    private string _issuer = options.Issuer;

    private TimeSpan _lifetime = options.AccessTokenLifetime;

    public string Build()
    {
        var signing = new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256);

        var token = new JwtSecurityToken(
            _issuer,
            _audience,
            _claims,
            DateTime.UtcNow,
            DateTime.UtcNow.Add(_lifetime),
            signing
        );

        return _handler.WriteToken(token);
    }

    public TokenBuilder WithAudience(string audience)
    {
        _audience = audience;

        return this;
    }

    public TokenBuilder WithIssuer(string issuer)
    {
        _issuer = issuer;

        return this;
    }

    public TokenBuilder WithLifetime(TimeSpan lifetime)
    {
        _lifetime = lifetime;

        return this;
    }

    public TokenBuilder WithRoles(params string[] roles)
    {
        foreach (var role in roles)
        {
            _claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return this;
    }

    public TokenBuilder WithSession(Guid id)
    {
        _claims.Add(new Claim(JwtRegisteredClaimNames.Jti, id.ToString()));

        return this;
    }

    public TokenBuilder WithUser(Guid id)
    {
        _claims.Add(new Claim(JwtRegisteredClaimNames.Sub, id.ToString()));

        return this;
    }
}