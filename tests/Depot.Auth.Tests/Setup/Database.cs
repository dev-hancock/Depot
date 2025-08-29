using Depot.Auth.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Depot.Auth.Tests.Setup;

public static partial class Database
{
    public static Task<T?> FindAsync<T>(params object[] keys) where T : class
    {
        return WithScope<T?>(context => context.FindAsync<T>(keys).AsTask());
    }

    public static async Task RemoveAsync(params object[] models)
    {
        await WithScope(context => context.RemoveRange(models));
    }

    public static async Task SeedAsync(params object[] models)
    {
        await WithScope(context => context.AddRangeAsync(models));
    }

    public static async Task Setup()
    {
        await WithScope(context => context.Database.EnsureCreatedAsync());
    }

    public static async Task Teardown()
    {
        await WithScope(context => context.Database.EnsureDeletedAsync());
    }

    private static Task<T?> WithScope<T>(Func<DbContext, Task<T?>> action)
    {
        return Service.Scoped<AuthDbContext, T?>(async context =>
        {
            var result = await action(context);

            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }

            return result;
        });
    }

    private static Task WithScope(Action<DbContext> action)
    {
        return Service.Scoped<AuthDbContext>(async context =>
        {
            action(context);

            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }
        });
    }
}