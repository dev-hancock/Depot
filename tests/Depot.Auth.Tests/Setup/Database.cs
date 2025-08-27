namespace Depot.Auth.Tests.Setup;

using Microsoft.EntityFrameworkCore;
using Persistence;

public static class Database
{
    public static ScopedDatabase CreateScope()
    {
        return new ScopedDatabase(GetDbContext());
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