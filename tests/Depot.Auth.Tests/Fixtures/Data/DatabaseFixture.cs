namespace Depot.Auth.Tests.Fixtures.Data;

using Microsoft.EntityFrameworkCore;
using Persistence;
using TUnit.Core.Interfaces;

public class DatabaseFixture : IAsyncInitializer, IAsyncDisposable
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture fixture { get; set; } = null!;

    private IDbContextFactory<AuthDbContext> Factory => fixture.GetService<IDbContextFactory<AuthDbContext>>();

    public async ValueTask DisposeAsync()
    {
        await Do(context => context.Database.EnsureDeletedAsync());
    }

    public async Task InitializeAsync()
    {
        await Do(context => context.Database.EnsureCreatedAsync());
    }

    private async Task<T?> Do<T>(Func<DbContext, Task<T?>> action)
    {
        var context = await Factory.CreateDbContextAsync();

        var result = await action(context);

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }

        return result;
    }

    private async Task Do(Func<DbContext, Task> action)
    {
        var context = await Factory.CreateDbContextAsync();

        await action(context);

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }

    public async Task SeedAsync(params object[] entities)
    {
        await Do(context => context.AddRangeAsync(entities));
    }

    public Task<T?> FindAsync<T>(params object[] keys) where T : class
    {
        return Do<T>(context => context.FindAsync<T>(keys).AsTask());
    }
}