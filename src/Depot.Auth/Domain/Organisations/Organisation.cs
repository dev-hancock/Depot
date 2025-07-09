namespace Depot.Auth.Domain.Organisations;

using Common;
using ErrorOr;
using Events;
using Tenants;

public class Organisation : AggregateRoot
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }


    public List<Tenant> Tenants { get; set; } = [];


    public static ErrorOr<Organisation> New(Guid creator, string name, Slug slug, TimeProvider time)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation();
        }

        return new Organisation
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug.Value,
            CreatedBy = creator,
            CreatedAt = time.GetUtcNow()
        };
    }

    public ErrorOr<Tenant> AddTenant(string name, Guid creator, TimeProvider time)
    {
        var exists = Tenants.Any(x => x.Name == name);

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

    public void AddTenant(Tenant tenant)
    {
        Tenants.Add(tenant);

        Events.Add(new TenantCreatedEvent(tenant));
    }

    public void Rename(string name)
    {
        Name = name;
    }
}