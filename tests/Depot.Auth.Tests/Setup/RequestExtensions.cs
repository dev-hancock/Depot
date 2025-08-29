namespace Depot.Auth.Tests.Setup;

public static partial class Requests
{
    public static RequestBuilder Login(LoginCommand command)
    {
        return Post("api/v1/auth/login", command);
    }

    public static RequestBuilder Logout(LogoutCommand command)
    {
        return Post("api/v1/auth/logout", command);
    }
}