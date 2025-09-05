namespace Depot.Auth.Tests.Setup;

public static class Arrange
{
    public static UserBuilder User => new();

    public static AccessTokenBuilder AccessToken => new();

    public static RefreshTokenBuilder RefreshToken => new();
}