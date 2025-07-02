namespace Depot.Auth.Domain;

public interface ITokenGenerator
{
    public AccessToken CreateAccessToken(User user, DateTime now);
}