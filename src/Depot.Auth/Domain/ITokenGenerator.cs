namespace Depot.Auth.Domain;

public interface ITokenGenerator
{
    public string CreateAccessToken(User user, DateTime now);
}