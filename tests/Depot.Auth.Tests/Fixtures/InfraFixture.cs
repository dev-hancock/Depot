namespace Depot.Auth.Tests.Fixtures;

public sealed class InfraFixture : IAsyncLifetime
{
    private readonly AuthDbFixture _auth = new();

    private readonly CacheDbFixture _cache = new();

    public string Auth => _auth.ConnectionString;

    public string Cache => _cache.ConnectionString;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_auth.InitializeAsync(), _cache.InitializeAsync());
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(_auth.DisposeAsync(), _cache.DisposeAsync());
    }
}