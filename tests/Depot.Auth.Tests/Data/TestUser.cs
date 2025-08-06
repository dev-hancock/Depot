namespace Depot.Auth.Tests.Data;

public sealed record TestUser(
    Guid Id,
    string Username,
    string Email,
    string Password,
    DateTime CreatedAt
);