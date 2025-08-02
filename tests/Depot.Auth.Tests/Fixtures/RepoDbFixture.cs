namespace Depot.Auth.Tests.Fixtures;

using DotNet.Testcontainers.Containers;
using Testcontainers.PostgreSql;

public class RepoDbFixture : IAsyncLifetime
{
    private IDatabaseContainer _db = null!;

    public string ConnectionString => _db.GetConnectionString();

    public async Task InitializeAsync()
    {
        var builder = new PostgreSqlBuilder()
            .WithDatabase("repo-test")
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