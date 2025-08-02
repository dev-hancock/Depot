namespace Depot.Auth.Tests.Fixtures;

public sealed class InfraFixture : IAsyncLifetime
{
    private readonly AuthDbFixture _auth = new();

    private readonly CacheDbFixture _cache = new();

    private readonly RepoDbFixture _repo = new();

    public string Auth => _auth.ConnectionString;

    public string Cache => _cache.ConnectionString;

    public string Repo => _repo.ConnectionString;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _auth.InitializeAsync(),
            _cache.InitializeAsync(),
            _repo.InitializeAsync());
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _auth.DisposeAsync(),
            _cache.DisposeAsync(),
            _repo.DisposeAsync());
    }
}