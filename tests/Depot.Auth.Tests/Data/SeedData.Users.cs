namespace Depot.Auth.Tests.Data;

using Domain.Auth;
using Domain.Interfaces;
using Domain.Users;
using Microsoft.Extensions.DependencyInjection;

public static partial class SeedData
{
    public static class Users
    {
        public async static Task<TestUser> SeedAsync(IServiceProvider services, Action<TestUser>? configure = null)
        {
            var user = TestUserFactory.CreateUser();

            configure?.Invoke(user);

            var hasher = services.GetRequiredService<ISecretHasher>();

            var entity = new User(
                new UserId(user.Id),
                Username.Create(user.Username),
                Email.Create(user.Email),
                Password.Create(hasher.Hash(user.Password)),
                user.CreatedAt);

            await AddEntity(services, entity);

            return user;
        }
    }
}