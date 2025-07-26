namespace Depot.Auth.Middleware;

using System.Security.Claims;

public interface IUserContext
{
    Guid UserId { get; }

    bool IsAuthenticated { get; }

    bool HasTenant => TenantId != Guid.Empty;

    Guid TenantId { get; }

    bool HasSession => SessionId != Guid.Empty;

    Guid SessionId { get; }

    void SetUser(Guid id);

    void SetTenant(Guid id);

    void SetSession(Guid id);

    void Clear();
}

public class UserContext : IUserContext
{
    public Guid UserId { get; private set; }

    public Guid TenantId { get; private set; }

    public Guid SessionId { get; private set; }

    public bool IsAuthenticated { get; private set; }

    public void SetUser(Guid userId)
    {
        UserId = userId;
        IsAuthenticated = true;
    }

    public void SetTenant(Guid id)
    {
        TenantId = id;
    }

    public void SetSession(Guid id)
    {
        SessionId = id;
    }

    public void Clear()
    {
        UserId = Guid.Empty;
        SessionId = Guid.Empty;
        TenantId = Guid.Empty;
        IsAuthenticated = false;
    }
}

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    private string? GetUser(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private string? GetTenant(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("tenant_id");
    }

    private string? GetSession(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("session_id");
    }

    public async Task Invoke(HttpContext context, IUserContext user)
    {
        user.Clear();

        var principal = context.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            if (Guid.TryParse(GetUser(principal), out var uid))
            {
                user.SetUser(uid);
            }

            if (Guid.TryParse(GetTenant(principal), out var tid))
            {
                user.SetTenant(tid);
            }

            if (Guid.TryParse(GetSession(principal), out var sid))
            {
                user.SetSession(sid);
            }
        }

        await _next(context);
    }
}