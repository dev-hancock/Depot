namespace Depot.Auth.Tests.Setup;

using Microsoft.EntityFrameworkCore;
using Persistence;

public static class Database
{
    public async static Task SeedAsync(params object[] models)
    {
        await WithScope(context => context.AddRangeAsync(models));
    }

    public static Task<T?> FindAsync<T>(params object[] keys) where T : class
    {
        return WithScope<T?>(context => context.FindAsync<T>(keys).AsTask());
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

    public async static Task RemoveAsync(params object[] models)
    {
        await WithScope(context => context.RemoveRange(models));
    }

    public async static Task Setup()
    {
        await WithScope(context => context.Database.EnsureCreatedAsync());
    }

    public async static Task Teardown()
    {
        await WithScope(context => context.Database.EnsureDeletedAsync());
    }
}