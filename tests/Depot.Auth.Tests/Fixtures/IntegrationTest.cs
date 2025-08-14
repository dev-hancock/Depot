namespace Depot.Auth.Tests.Features.Auth.Login;

using Bogus;
using DotNet.Testcontainers.Containers;
using Factories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Testcontainers.PostgreSql;

public class PostgreSqlFixture(string database) : IAsyncLifetime
{
    private IDatabaseContainer _db = null!;

    public string ConnectionString => _db.GetConnectionString();

    public async Task InitializeAsync()
    {
        var builder = new PostgreSqlBuilder()
            .WithDatabase(database)
            .WithUsername("test")
            .WithPassword("test");

        _db = builder.Build();

        await _db.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
    }
}

public class IntegrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlFixture _auth = new("auth");

    private readonly PostgreSqlFixture _cache = new("cache");

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

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationFixture>;

[Collection("Integration")]
public abstract class IntegrationTest
{
    protected static readonly Faker Faker = new();

    public IntegrationTest(IntegrationFixture fixture)
    {
        var factory = new TestAppFactory(fixture);

        Services = factory.Services;

        Client = factory.CreateClient();

        Db = Services.GetRequiredService<AuthDbContext>();

        Cache = Services.GetRequiredService<IDistributedCache>();
    }

    public IDistributedCache Cache { get; }

    protected IServiceProvider Services { get; }

    protected HttpClient Client { get; }

    protected AuthDbContext Db { get; }
}