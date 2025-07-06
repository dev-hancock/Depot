namespace Depot.Auth.Domain.Events;

using Common;
using Tenants;
using Users;

public sealed record PasswordChangedEvent(User User) : IEvent;

public sealed record EmailChangedEvent(User User) : IEvent;

public sealed record TenantCreatedEvent(Tenant Tenant) : IEvent;