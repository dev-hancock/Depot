using System.Security.Claims;
using Depot.Auth.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Depot.Auth.Tests.Setup;

public class AccessTokenBuilder
{
    private static readonly JwtOptions Options = Global.GetService<IOptions<JwtOptions>>().Value;

    private readonly List<Claim> _claims = [];

    private readonly JwtSecurityTokenHandler _handler = new();

    private readonly SecurityKey _key = Global.GetService<SecurityKey>();

    private string _audience = Options.Audience;

    private string _issuer = Options.Issuer;

    private TimeSpan _lifetime = Options.AccessTokenLifetime;

    public string Build()
    {
        var signing = new SigningCredentials(_key, SecurityAlgorithms.EcdsaSha256);

        var token = new JwtSecurityToken(
            _issuer,
            _audience,
            _claims,
            DateTimeOffset.UtcNow.DateTime,
            DateTime.UtcNow.Add(_lifetime),
            signing
        );

        return _handler.WriteToken(token);
    }

    public AccessTokenBuilder WithAudience(string audience)
    {
        _audience = audience;

        return this;
    }

    public AccessTokenBuilder WithIssuer(string issuer)
    {
        _issuer = issuer;

        return this;
    }

    public AccessTokenBuilder WithLifetime(TimeSpan lifetime)
    {
        _lifetime = lifetime;

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

    public AccessTokenBuilder WithSession(Guid id)
    {
        _claims.Add(new Claim("sid", id.ToString()));

        return this;
    }

    public AccessTokenBuilder WithUser(Guid id)
    {
        _claims.Add(new Claim("sub", id.ToString()));

        return this;
    }

    public AccessTokenBuilder WithUser(User user)
    {
        return WithUser(user.Id.Value);
    }
}