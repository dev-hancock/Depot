using Depot.Auth.Domain.Auth;
using Depot.Auth.Domain.Users;
using Depot.Auth.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Depot.Auth.Services;

public interface ITokenService
{
    AccessToken GetAccessToken(User user, Session session, DateTimeOffset now);

    RefreshToken GetRefreshToken(DateTimeOffset now);
}

public class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    private readonly TokenGenerator _tokens;

    public TokenService(IOptions<JwtOptions> options, SecurityKey key)
    {
        _options = options.Value;
        _tokens = new TokenGenerator(_options.Issuer, _options.Audience, key);
    }

    public AccessToken GetAccessToken(User user, Session session, DateTimeOffset now)
    {
        var expiry = now + _options.AccessTokenLifetime;

        var token = _tokens.GenerateAccessToken(
            user.Id,
            session.Id,
            [], // TODO: roles
            now,
            expiry,
            session.Version);

        return AccessToken.Create(token, expiry);
    }

    public RefreshToken GetRefreshToken(DateTimeOffset now)
    {
        var expiry = now + _options.RefreshTokenLifetime;

        var token = _tokens.GenerateRefreshToken();

        return RefreshToken.Create(token, expiry);
    }
}