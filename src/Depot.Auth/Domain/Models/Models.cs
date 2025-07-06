namespace Depot.Auth.Domain.Models;

using ErrorOr;

public interface IEvent
{
}

public sealed record PasswordChangedEvent(User User) : IEvent;

public sealed record EmailChangedEvent(User User) : IEvent;

public sealed record TenantCreatedEvent(Tenant Tenant) : IEvent;

public abstract class AggregateRoot
{
    // TODO: Placeholder
    public List<IEvent> Events { get; set; } = [];
}

public class Organisation : AggregateRoot
{
    public Guid Id { get; set; }


    public string Name { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }


    public List<Tenant> Tenants { get; set; } = [];


    public static ErrorOr<Organisation> New(string name, User creator, TimeProvider time)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation();
        }

        return new Organisation
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedBy = creator.Id,
            CreatedAt = time.GetUtcNow()
        };
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

public class Email
{
    public string Value { get; set; } = null!;
}

public class User : AggregateRoot
{
    public Guid Id { get; set; }


    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }


    public List<Membership> Memberships { get; set; } = [];


    public static ErrorOr<User> New(string username, Password password, Email email, TimeProvider time)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Error.Validation();
        }

        return new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Password = password.Encoded,
            Email = email.Value,
            CreatedAt = time.GetUtcNow()
        };
    }

    public void ChangePassword(string current, Password updated)
    {
        // Check old password

        Password = updated.Encoded;

        Events.Add(new PasswordChangedEvent(this));
    }

    public void ChangeEmail(Email email)
    {
        Email = email.Value;

        Events.Add(new EmailChangedEvent(this));
    }
}

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

public class Permission
{
    public Guid Id { get; set; }


    public string Name { get; set; } = null!;


    public List<RolePermission> RolePermissions { get; set; } = [];
}

public class RolePermission
{
    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }


    public Role Role { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}

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