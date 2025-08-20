namespace Depot.Auth.Tests.Utils;

using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;

public static class JwtTokenExtensions
{
    private static readonly JwtSecurityTokenHandler Handler = new();

    public static string GetClaimValue(this string token, string type)
    {
        return Handler.ReadJwtToken(token).Claims.Single(x => x.Type == type).Value;
    }

    public static T? GetClaimValue<T>(this string token, string type)
    {
        return (T?)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(GetClaimValue(token, type));
    }
}