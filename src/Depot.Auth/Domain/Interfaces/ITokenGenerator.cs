namespace Depot.Auth.Domain.Interfaces;

using Auth;
using Users;

public interface ITokenGenerator
{
    public AccessToken CreateAccessToken(User user, DateTime now);
}