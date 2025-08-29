using Depot.Auth.Domain.Auth;
using Depot.Auth.Domain.Interfaces;

namespace Depot.Auth.Mappings;

public static class TokenMappings
{
    public static AccessToken ToAccessToken(this Token token)
    {
        return AccessToken.Create(token.Value, token.Expiry);
    }

    public static RefreshToken ToRefreshToken(this Token token)
    {
        return RefreshToken.Create(token.Value, token.Expiry);
    }
}