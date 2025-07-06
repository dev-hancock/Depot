namespace Depot.Auth.Domain.Users;

using Tenants;

public class Membership
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public Guid TenantId { get; set; }


    public User User { get; set; } = null!;

    public Role Role { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;


    public void SetRole(Role role)
    {
        Role = role;
    }
}