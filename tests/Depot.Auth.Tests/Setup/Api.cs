using Depot.Auth.Features.Auth.Refresh;

namespace Depot.Auth.Tests.Setup;

public static class Api
{
    public static RequestBuilder Login(LoginCommand command)
    {
        return Requests.Post("api/v1/auth/login", command);
    }

    public static RequestBuilder Logout(LogoutCommand command)
    {
        return Requests.Post("api/v1/auth/logout", command);
    }

    public static RequestBuilder Refresh(RefreshCommand command)
    {
        return Requests.Post("api/v1/auth/refresh", command);
    }
}