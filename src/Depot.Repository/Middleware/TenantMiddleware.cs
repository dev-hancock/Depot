namespace Depot.Repository.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var tenant = context.User.FindFirst("tenant_id")?.Value;

        if (tenant is not null)
        {
            context.Items["TenantId"] = tenant;
        }

        await _next(context);
    }
}