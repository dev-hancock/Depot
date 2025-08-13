namespace Depot.Auth.Tests.Data.Models;

public sealed record TestSession(
    Guid Id,
    string AccessToken,
    string RefreshToken,
    bool IsRevoked,
    DateTime ExpiresAt);