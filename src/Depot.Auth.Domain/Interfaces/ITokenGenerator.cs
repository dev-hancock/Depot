namespace Depot.Auth.Domain.Interfaces;

public sealed record Token(string Value, DateTime Expiry);

public interface ITokenGenerator
{
    Token GenerateAccessToken(Guid sub, Guid jti, string[] roles, DateTime now);

    Token GenerateRefreshToken(DateTime now);
}