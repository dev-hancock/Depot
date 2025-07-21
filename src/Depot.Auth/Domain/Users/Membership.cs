namespace Depot.Auth.Domain.Users;

using Auth;
using Tenants;

public class Membership
{
    public UserId UserId { get; set; }

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