namespace Depot.Auth.Middleware;

using System.Security.Claims;

public interface IUserContext
{
    Guid UserId { get; }

    bool IsAuthenticated { get; }

    void Set(Guid id);
}

public class UserContext : IUserContext
{
    public Guid UserId { get; private set; }

    public bool IsAuthenticated { get; private set; }

    public void Set(Guid id)
    {
        UserId = id;
        IsAuthenticated = true;
    }
}

public class UserMiddleware
{
    private readonly RequestDelegate _next;

    public UserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IUserContext user)
    {
        var id = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(id))
        {
            user.Set(Guid.Parse(id));
        }

        await _next(context);
    }
}