namespace Depot.Auth.Tests.Setup;

using Microsoft.Extensions.DependencyInjection;

public static class Service
{
    public static TService Get<TService>() where TService : notnull
    {
        return GetService<TService>(Global.Application.Services);
    }

    private static IServiceScope CreateScope()
    {
        return Global.Application.Services.CreateScope();
    }

    public async static Task Scoped<TService>(Func<TService, Task> action) where TService : notnull
    {
        using var scope = CreateScope();

        await action(GetService<TService>(scope.ServiceProvider));
    }

    public async static Task<TResult> Scoped<TService, TResult>(Func<TService, Task<TResult>> action) where TService : notnull
    {
        using var scope = CreateScope();

        return await action(GetService<TService>(scope.ServiceProvider));
    }

    private static TService GetService<TService>(IServiceProvider services) where TService : notnull
    {
        return services.GetRequiredService<TService>();
    }

    public static ScopedService<TService> Scoped<TService>() where TService : notnull
    {
        var scope = CreateScope();

        return new ScopedService<TService>(scope, GetService<TService>(scope.ServiceProvider));
    }
}