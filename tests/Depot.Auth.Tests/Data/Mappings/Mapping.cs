namespace Depot.Auth.Tests.Data.Mappings;

using Abstractions;
using Domain.Auth;
using Domain.Interfaces;
using Domain.Users;
using Microsoft.Extensions.DependencyInjection;
using Models;

public static class Mapping
{
    public static readonly MappingDelegate<TestUser, User> User = (sp, x) =>
    {
        var hasher = sp.GetRequiredService<ISecretHasher>();

        return new User(
            new UserId(x.Id),
            new Username(x.Username),
            new Email(x.Email),
            new Password(hasher.Hash(x.Password)),
            x.Sessions
                .Select(y => new Session(
                    new SessionId(y.Id),
                    new UserId(x.Id),
                    new RefreshToken(y.RefreshToken, y.ExpiresAt),
                    y.IsRevoked
                ))
                .ToArray(),
            x.CreatedAt
        );
    };
}