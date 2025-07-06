namespace Depot.Auth.Domain.Tenants;

using ErrorOr;
using Organisations;
using Users;

public class Tenant
{
    public Guid Id { get; set; }

    public Guid OrganisationId { get; set; }


    public string Name { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }


    public Organisation? Organisation { get; set; }

    public List<Role> Roles { get; set; } = [];

    public List<Membership> Memberships { get; set; } = [];


    public static ErrorOr<Tenant> New(string name, User creator, TimeProvider time)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation();
        }

        return new Tenant
        {
            Name = name,
            CreatedBy = creator.Id,
            CreatedAt = time.GetUtcNow()
        };
    }

    public void AddRole(Role role)
    {
        Roles.Add(role);
    }

    public void AddMembership(User user, Role role)
    {
    }

    public void AssignRole(User user, Role role)
    {
    }
}