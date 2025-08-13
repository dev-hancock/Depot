namespace Depot.Auth.Tests.Features.Auth.Login;

using Depot.Auth.Features.Auth.Login;
using Depot.Auth.Features.Auth.Logout;
using Depot.Auth.Features.Auth.Refresh;
using Depot.Auth.Features.Auth.Register;
using Refit;

public interface IAuthApi
{
    Task<LoginResponse> Login([Body] LoginCommand command);

    Task<LoginResponse> Refresh([Body] RefreshCommand command);

    Task Logout([Body] LogoutCommand command);

    Task<LoginResponse> Register([Body] RegisterCommand command);
}