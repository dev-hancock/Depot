namespace Depot.Auth.Tests.Data.Extensions;

using Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class DatabaseExtensions
{
    public async static Task<TSpec> SeedAsync<TSpec, TEntity>(this IBuilder<TSpec, TEntity> builder, IServiceProvider services)
        where TEntity : class
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<DbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var contract = builder.Build(services);

        var entity = builder.Mapping(services, contract);

        context.Set<TEntity>().Add(entity);

        await context.SaveChangesAsync();

        return contract;
    }
}