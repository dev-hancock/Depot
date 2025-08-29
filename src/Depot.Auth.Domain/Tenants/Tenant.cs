using Depot.Auth.Domain.Auth;
using Depot.Auth.Domain.Common;
using Depot.Auth.Domain.Organisations;
using Depot.Auth.Domain.Users;
using ErrorOr;

namespace Depot.Auth.Domain.Tenants;

public class Tenant
{
    private Tenant(string name, UserId creator, DateTimeOffset createdAt)
    {
        Name = name;
        Slug = Slug.Create(name);
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

    public UserId CreatedBy { get; set; }


    public Organisation? Organisation { get; set; }

    public List<Role> Roles { get; set; } = [];

    public List<Membership> Memberships { get; set; } = [];

    public static ErrorOr<Tenant> New(string name, UserId creator, TimeProvider time)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation();
        }

        return new Tenant(name, creator, time.GetUtcNow());
    }

    public void AddMembership(User user, Role role) { }

    public void AddRole(Role role)
    {
        Roles.Add(role);
    }

    public void AssignRole(User user, Role role) { }
}

public static class Tenants
{
    public static Tenant Personal(UserId creator, TimeProvider time)
    {
        return Tenant.New("Personal", creator, time).Value;
    }
}