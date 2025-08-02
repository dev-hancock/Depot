namespace Depot.Auth.Domain.Organisations;

using Auth;
using Common;
using ErrorOr;
using Tenants;
using Users.Events;

public class Organisation : Root
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Slug Slug { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public UserId CreatedBy { get; set; }


    public List<Tenant> Tenants { get; set; } = [];


    public static ErrorOr<Organisation> New(string name, UserId creator, TimeProvider time)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation();
        }

        return new Organisation
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = Slug.Create(name),
            CreatedBy = creator,
            CreatedAt = time.GetUtcNow()
        };
    }

    public ErrorOr<Tenant> AddTenant(string name, UserId creator, TimeProvider time)
    {
        var exists = Tenants.Any(x => x.Slug.Value == name);

        if (exists)
        {
            return Error.Conflict();
        }

        var result = Tenant.New(name, creator, time);

        if (!result.IsError)
        {
            Tenants.Add(result.Value);
        }

        return result;
    }

    public ErrorOr<Tenant> AddTenant(Tenant tenant)
    {
        if (Tenants.Any(x => x.Slug == tenant.Slug))
        {
            return Error.Conflict();
        }

        Tenants.Add(tenant);

        Raise(new TenantCreatedEvent(tenant));

        return tenant;
    }

    public void Rename(string name)
    {
        Name = name;
    }
}