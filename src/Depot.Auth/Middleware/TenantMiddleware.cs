namespace Depot.Auth.Middleware;

public interface ITenantContext
{
    public Guid TenantId { get; }

    public void Set(Guid tenantId);
}

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }

    public void Set(Guid tenantId)
    {
        TenantId = tenantId;
    }
}

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ITenantContext tenant)
    {
        var id = context.User.FindFirst("tenant_id")?.Value;

        if (!string.IsNullOrWhiteSpace(id))
        {
            tenant.Set(Guid.Parse(id));
        }

        await _next(context);
    }
}