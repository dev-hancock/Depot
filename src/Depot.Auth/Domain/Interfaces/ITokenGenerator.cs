namespace Depot.Auth.Domain.Interfaces;

using Auth;
using Users;

public interface ITokenGenerator
{
    public AccessToken GenerateAccessToken(User user, SessionId session, DateTime now);

    public RefreshToken GenerateRefreshToken(DateTime now);
}