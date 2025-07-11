namespace Depot.Auth.Domain.Tenants;

using Common;
using ErrorOr;
using Organisations;
using Users;

public class Tenant
{
    private Tenant(string name, Guid creator, DateTimeOffset createdAt)
    {
        Name = name;
        Slug = name;
        CreatedAt = createdAt;
        CreatedBy = creator;
    }

    public Tenant()
    {
        // TEMP
    }

    public Guid Id { get; set; }

    public Guid? OrganisationId { get; set; }


    public string Name { get; set; } = null!;

    public Slug Slug { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }


    public Organisation? Organisation { get; set; }

    public List<Role> Roles { get; set; } = [];

    public List<Membership> Memberships { get; set; } = [];

    public static ErrorOr<Tenant> New(string name, Guid creator, TimeProvider time)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation();
        }

        return new Tenant(name, creator, time.GetUtcNow());
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

public static class Tenants
{
    public static Tenant Personal(Guid creator, TimeProvider time)
    {
        return Tenant.New("Personal", creator, time).Value;
    }
}