namespace Depot.Auth.Tests.Setup;

public static class TokenExtensions
{
    public static string? GetClaim(this JwtSecurityToken token, string type)
    {
        return token.Claims.Single(x => x.Type == type)?.Value;
    }

    public static Guid GetSessionId(this JwtSecurityToken token)
    {
        return Guid.Parse(token.Claims.Single(x => x.Type == "sid").Value);
    }

    public static Guid GetUserId(this JwtSecurityToken token)
    {
        return Guid.Parse(token.Claims.Single(x => x.Type == "sub").Value);
    }
}