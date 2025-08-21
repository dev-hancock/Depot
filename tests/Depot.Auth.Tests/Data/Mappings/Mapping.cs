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
                    new SessionId(x.Id),
                    new UserId(y.UserId),
                    new RefreshToken(y.RefreshToken, y.ExpiresAt),
                    y.IsRevoked
                ))
                .ToArray(),
            x.CreatedAt
        );
    };
}

public class UserSeeder : ISeeder<TestUser, User>
{
    private readonly MappingDelegate<TestUser, User> _mapping;

    public UserSeeder(MappingDelegate<TestUser, User> mapping)
    {
        _mapping = mapping;
    }

    public User Invoke(IServiceProvider services, TestUser model)
    {
        return _mapping(services, model);
    }
}

public interface ISeeder<TestUser, User> { }

public static class Database { }