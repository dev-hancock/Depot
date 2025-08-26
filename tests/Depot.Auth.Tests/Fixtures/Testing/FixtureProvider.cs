namespace Depot.Auth.Tests.Fixtures.Testing;

using Microsoft.Extensions.DependencyInjection;
using TUnit.Core.Interfaces;

public abstract class FixtureProvider : IAsyncInitializer, IAsyncDisposable
{
    private readonly ServiceCollection _services = [];

    private ServiceProvider _provider = null!;

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        Configure();

        _provider = _services.BuildServiceProvider();

        foreach (var descriptor in _services)
        {
            var service = _provider.GetService(descriptor.ServiceType);

            if (service is IAsyncInitializer x)
            {
                await x.InitializeAsync();
            }
        }
    }

    protected void Add<T>(Func<T> factory) where T : class
    {
        _services.AddSingleton(factory.Invoke());
    }

    protected abstract void Configure();

    protected T Get<T>() where T : class
    {
        return _provider.GetRequiredService<T>();
    }
}