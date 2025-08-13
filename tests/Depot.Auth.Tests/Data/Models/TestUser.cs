namespace Depot.Auth.Tests.Data.Models;

public sealed record TestUser(
    Guid Id,
    string Username,
    string Email,
    string Password,
    TestSession[] Sessions,
    DateTime CreatedAt
);