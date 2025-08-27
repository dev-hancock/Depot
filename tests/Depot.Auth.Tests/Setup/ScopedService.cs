namespace Depot.Auth.Tests.Setup;

using Microsoft.Extensions.DependencyInjection;

public class ScopedService<T>(IServiceScope scope, T value) : IDisposable
{
    public T Value { get; } = value;

    public void Dispose()
    {
        scope.Dispose();
    }
}