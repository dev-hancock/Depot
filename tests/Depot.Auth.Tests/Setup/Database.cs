using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Depot.Auth.Tests.Setup;

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
public class Database<TContext> where TContext : DbContext
{
    private static readonly SemaphoreSlim Lock = new(1, 1);

    private static volatile bool s_initialised;

    private static readonly Database<TContext> Shared = new();

    public static Task<Database<TContext>> Instance => CreateAsync();

    public async Task<T> With<T>(Func<TContext, Task<T>> action)
    {
        using var scope = GetScope();

        await using var context = GetDbContext(scope.ServiceProvider);

        var result = await action(context);

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }

        return result;
    }

    public async Task With(Func<TContext, Task> action)
    {
        using var scope = GetScope();

        await using var context = GetDbContext(scope.ServiceProvider);

        await action(context);

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }

    private static async Task<Database<TContext>> CreateAsync()
    {
        if (s_initialised)
        {
            return Shared;
        }

        await Lock.WaitAsync();

        try
        {
            if (!s_initialised)
            {
                using var scope = GetScope();

                await using var context = GetDbContext(scope.ServiceProvider);

                await context.Database.EnsureCreatedAsync();
            }

            s_initialised = true;
        }
        finally
        {
            Lock.Release();
        }

        return Shared;
    }

    private static TContext GetDbContext(IServiceProvider services)
    {
        return services.GetRequiredService<TContext>();
    }

    private static IServiceScope GetScope()
    {
        return Global.Application.Services.CreateScope();
    }
}