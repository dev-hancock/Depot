using Microsoft.Extensions.DependencyInjection;

namespace Depot.Auth.Tests.Setup;

public class ScopedService<T>(IServiceScope scope, T value) : IDisposable
{
    public T Value { get; } = value;

    public void Dispose()
    {
        scope.Dispose();
    }
}