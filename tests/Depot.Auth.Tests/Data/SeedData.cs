namespace Depot.Auth.Tests.Data;

using Microsoft.Extensions.DependencyInjection;
using Persistence;

public static partial class SeedData
{
    private async static Task AddEntity<T>(IServiceProvider services, T entity) where T : class
    {
        using var scope = services.CreateScope();

        await using var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        await context.Database.EnsureCreatedAsync();

        context.Set<T>().Add(entity);

        await context.SaveChangesAsync();
    }
}