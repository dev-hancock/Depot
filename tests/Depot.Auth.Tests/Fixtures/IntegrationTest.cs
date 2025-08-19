namespace Depot.Auth.Tests.Features.Auth.Login;

using System.Net.Http.Json;
using Bogus;
using Depot.Auth.Persistence;
using DotNet.Testcontainers.Containers;
using Factories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
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
public abstract class IntegrationTest : IDisposable
{
    protected static readonly Faker Faker = new();

    private readonly IServiceScope _scope;

    public IntegrationTest(IntegrationFixture fixture)
    {
        var factory = new TestAppFactory(fixture);

        Client = factory.CreateClient();

        _scope = factory.Services.CreateScope();

        Services = _scope.ServiceProvider;

        Db = Services.GetRequiredService<AuthDbContext>();

        Cache = Services.GetRequiredService<IDistributedCache>();
    }

    public IDistributedCache Cache { get; }

    protected IServiceProvider Services { get; }

    protected HttpClient Client { get; }

    protected AuthDbContext Db { get; }

    public void Dispose()
    {
        _scope.Dispose();
    }
}

public static class Requests
{
    private static HttpRequestMessage Create(
        HttpMethod method,
        string uri,
        object? payload = null,
        string? token = null,
        params (string Key, string Value)[] headers)
    {
        var request = new HttpRequestMessage(method, uri);

        if (payload != null)
        {
            request.Content = JsonContent.Create(payload);
        }

        if (headers is { Length: > 0 })
        {
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        if (token != null)
        {
            request.Headers.Add("Authorization", $"Bearer {token}");
        }

        return request;
    }

    public static HttpRequestMessage Put<T>(
        string uri, T payload, string? token = null,
        params (string Key, string Value)[] headers)
    {
        return Create(HttpMethod.Put, uri, payload, token, headers);
    }

    public static HttpRequestMessage Post<T>(
        string uri, T payload, string? token = null,
        params (string Key, string Value)[] headers)
    {
        return Create(HttpMethod.Post, uri, payload, token, headers);
    }

    public static HttpRequestMessage Get(
        string uri, string? token = null,
        params (string Key, string Value)[] headers)
    {
        return Create(HttpMethod.Get, uri, null, token, headers);
    }

    public static HttpRequestMessage Delete(
        string uri, string? token = null,
        params (string Key, string Value)[] headers)
    {
        return Create(HttpMethod.Delete, uri, null, token, headers);
    }
}