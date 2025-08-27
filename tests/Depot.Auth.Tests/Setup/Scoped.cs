namespace Depot.Auth.Tests.Setup;

using Microsoft.Extensions.DependencyInjection;

public static class Scoped
{
    public async static Task Service<T>(Func<T, Task> action) where T : notnull
    {
        using var scope = Service<T>();

        await action(scope.Value);
    }

    private static IServiceScope CreateScope()
    {
        return Global.Application.Services.CreateScope();
    }

    private static T GetService<T>(IServiceScope scope) where T : notnull
    {
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    public static ScopedService<T> Service<T>() where T : notnull
    {
        var scope = CreateScope();

        return new ScopedService<T>(scope, GetService<T>(scope));
    }
}