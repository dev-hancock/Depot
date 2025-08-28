namespace Depot.Auth.Tests.Setup;

using Microsoft.EntityFrameworkCore;
using Persistence;

public static class Database
{
    public async static Task SeedAsync(params object[] models)
    {
        await using var context = GetDbContext();

        context.AddRange(models);

        await context.SaveChangesAsync();
    }

    public async static Task<T?> FindAsync<T>(params object[] keys) where T : class
    {
        await using var context = GetDbContext();

        return await context.FindAsync<T>(keys);
    }

    private static DbContext GetDbContext()
    {
        return Global.Service<IDbContextFactory<AuthDbContext>>().CreateDbContext();
    }

    public async static Task Setup()
    {
        await GetDbContext().Database.EnsureCreatedAsync();
    }

    public async static Task Teardown()
    {
        await GetDbContext().Database.EnsureDeletedAsync();
    }
}