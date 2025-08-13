namespace Depot.Auth.Domain.Users.Events;

using Auth;
using Mestra.Abstractions;
using Tenants;

public sealed record PasswordChangedEvent(User User) : INotification;

public sealed record EmailChangedEvent(User User) : INotification;

public sealed record TenantCreatedEvent(Tenant Tenant) : INotification;

public record SessionCreatedEvent(SessionId SessionId, DateTime ExpiresAt) : INotification;

public record SessionRevokedEvent(SessionId SessionId) : INotification;

public record SessionRefreshedEvent(SessionId SessionId, DateTime ExpiresAt) : INotification;