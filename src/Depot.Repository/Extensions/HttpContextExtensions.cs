namespace Depot.Repository.Extensions;

using System.Security.Claims;

public static class HttpContextExtensions
{
    public static Guid? GetTenantId(this HttpContext context)
    {
        var id = context.User.FindFirst("tenant_id")?.Value;

        if (id is null)
        {
            return null;
        }

        return Guid.Parse(id);
    }

    public static Guid? GetUserId(this HttpContext context)
    {
        var id = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (id is null)
        {
            return null;
        }

        return Guid.Parse(id);
    }

    public static string? GetUser(this HttpContext context)
    {
        return context.User.Identity?.Name;
    }
}