using Depot.Auth.Domain.Tenants;
using Mestra.Abstractions;

namespace Depot.Auth.Domain.Users.Events;

public sealed record PasswordChangedEvent(User User) : INotification;

public sealed record EmailChangedEvent(User User) : INotification;

public sealed record TenantCreatedEvent(Tenant Tenant) : INotification;

public record SessionCreatedEvent(Guid Id, int Version) : INotification;

public record SessionRevokedEvent(Guid Id, int Version) : INotification;

public record SessionRefreshedEvent(Guid Id, int Version) : INotification;