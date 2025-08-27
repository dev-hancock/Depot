namespace Depot.Auth.Tests.Setup;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Options;

public class AccessTokenBuilder(JwtOptions options, SecurityKey key)
{
    private readonly List<Claim> _claims = new();

    private readonly JwtSecurityTokenHandler _handler = new();

    private string _audience = options.Audience;

    private string _issuer = options.Issuer;

    private TimeSpan _lifetime = options.AccessTokenLifetime;

    public AccessTokenBuilder WithSession(Guid id)
    {
        _claims.Add(new Claim(JwtRegisteredClaimNames.Jti, id.ToString()));

        return this;
    }

    public AccessTokenBuilder WithUser(Guid id)
    {
        _claims.Add(new Claim(JwtRegisteredClaimNames.Sub, id.ToString()));

        return this;
    }

    public AccessTokenBuilder WithRoles(params string[] roles)
    {
        foreach (var role in roles)
        {
            _claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return this;
    }

    public AccessTokenBuilder WithIssuer(string issuer)
    {
        _issuer = issuer;

        return this;
    }

    public AccessTokenBuilder WithAudience(string audience)
    {
        _audience = audience;

        return this;
    }

    public AccessTokenBuilder WithLifetime(TimeSpan lifetime)
    {
        _lifetime = lifetime;

        return this;
    }

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
}