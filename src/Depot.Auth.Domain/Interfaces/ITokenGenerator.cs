namespace Depot.Auth.Domain.Interfaces;

public sealed record Token(string Value, DateTimeOffset Expiry);

public interface ITokenGenerator
{
    Token GenerateAccessToken(Guid sub, Guid sid, string[] roles, DateTimeOffset now, int version = 0);

    Token GenerateRefreshToken(DateTime now, int length = 64);
}