namespace Depot.Auth.Mapping;

using Domain.Auth;
using Domain.Interfaces;

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