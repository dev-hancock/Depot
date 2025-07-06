namespace Depot.Auth.Domain.Users;

using Tenants;

public class Role
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }


    public string Name { get; set; } = null!;


    public Tenant Tenant { get; set; } = null!;

    public List<RolePermission> Permissions { get; set; } = [];

    public List<Membership> Memberships { get; set; } = [];


    public static Role New()
    {
        return new Role();
    }

    public void AddPermission(RolePermission permission)
    {
        Permissions.Add(permission);
    }

    public void RemovePermission(RolePermission permission)
    {
        Permissions.Remove(permission);
    }

    public void SetPermissions(List<RolePermission> permissions)
    {
        Permissions = permissions;
    }
}