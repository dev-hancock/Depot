namespace Depot.Auth.Tests.Fixtures.Infrastructure;

using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

public class PostgreSqlFixture : IAsyncInitializer, IAsyncDisposable
{
    private readonly PostgreSqlBuilder _builder = new();

    private readonly PostgreSqlContainer _db;

    protected PostgreSqlFixture(string database)
    {
        _builder = _builder.WithDatabase(database);

        _db = _builder.Build();
    }

    public string ConnectionString => _db.GetConnectionString();

    public async ValueTask DisposeAsync()
    {
        await _db.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _db.StartAsync();
    }
}