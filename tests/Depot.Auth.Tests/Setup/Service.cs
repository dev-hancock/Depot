using Microsoft.Extensions.DependencyInjection;

namespace Depot.Auth.Tests.Setup;

public static class Service
{
    public static TService Get<TService>() where TService : notnull
    {
        return GetService<TService>(Global.Application.Services);
    }

    public static async Task Scoped<TService>(Func<TService, Task> action) where TService : notnull
    {
        using var scope = CreateScope();

        await action(GetService<TService>(scope.ServiceProvider));
    }

    public static async Task<TResult> Scoped<TService, TResult>(Func<TService, Task<TResult>> action) where TService : notnull
    {
        using var scope = CreateScope();

        return await action(GetService<TService>(scope.ServiceProvider));
    }

    public static ScopedService<TService> Scoped<TService>() where TService : notnull
    {
        var scope = CreateScope();

        return new ScopedService<TService>(scope, GetService<TService>(scope.ServiceProvider));
    }

    private static IServiceScope CreateScope()
    {
        return Global.Application.Services.CreateScope();
    }

    private static TService GetService<TService>(IServiceProvider services) where TService : notnull
    {
        return services.GetRequiredService<TService>();
    }
}