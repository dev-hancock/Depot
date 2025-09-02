namespace Depot.Auth.Tests.Setup;

public static class TokenExtensions
{
    public static string GetSessionId(this JwtSecurityToken token)
    {
        return token.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
    }

    public static string GetUserId(this JwtSecurityToken token)
    {
        return token.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
    }
}