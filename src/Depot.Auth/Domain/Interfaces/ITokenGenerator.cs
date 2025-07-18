namespace Depot.Auth.Domain.Interfaces;

using Auth;
using Users;

public interface ITokenGenerator
{
    public AccessToken GenerateAccessToken(User user, DateTime now);

    public RefreshToken GenerateRefreshToken(DateTime now);
}