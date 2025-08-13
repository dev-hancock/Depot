namespace Depot.Auth.Tests.Data;

using Builders;
using Domain.Auth;
using Domain.Interfaces;
using Domain.Users;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Persistence;

public static class SeedingExtensions
{
    public async static Task<TestUser> SeedAsync(this UserBuilder builder, IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        await context.Database.EnsureCreatedAsync();

        var user = builder.Build(services);

        var hasher = services.GetRequiredService<ISecretHasher>();

        var entity = new User(
            new UserId(user.Id),
            new Username(user.Username),
            new Email(user.Email),
            new Password(hasher.Hash(user.Password)),
            user.Sessions
                .Select(x => new Session(
                    new SessionId(x.Id),
                    new UserId(user.Id),
                    new RefreshToken(x.RefreshToken, x.ExpiresAt),
                    x.IsRevoked
                ))
                .ToArray(),
            user.CreatedAt
        );

        context.Users.Add(entity);

        return user;
    }
}