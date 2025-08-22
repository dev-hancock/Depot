namespace Depot.Auth.Tests.Features.Auth.Login;

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using Bogus;
using Data.Builders;
using Depot.Auth.Persistence;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Options;
using Testcontainers.PostgreSql;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationFixture>;

public class ArrangeFixture(ApplicationFixture application)
{
    public UserBuilder User => new(application);
}

public class DatabaseFixture(ApplicationFixture application) : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return Do(context => context.Database.EnsureCreatedAsync());
    }

    public Task DisposeAsync()
    {
        return Do(context => context.Database.EnsureDeletedAsync());
    }

    private async Task<T?> Do<T>(Func<DbContext, Task<T?>> action)
    {
        using var scope = application.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<DbContext>();

        var result = await action(context);

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }

        return result;
    }

    private async Task Do(Func<DbContext, Task> action)
    {
        using var scope = application.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<DbContext>();

        await action(context);

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }

    public Task SeedAsync(params object[] entities)
    {
        return Do(context => context.AddRangeAsync(entities));
    }

    public Task<T?> FindAsync<T>(params object[] keys) where T : class
    {
        return Do<T>(context => context.FindAsync<T>(keys).AsTask());
    }
}

[Collection("Integration")]
public abstract class IntegrationTest(IntegrationFixture fixture)
{
    protected IntegrationFixture Fixture { get; } = fixture;
}

[Collection("Integration")]
public class IntegrationFixture : IAsyncLifetime, IDisposable
{
    public readonly Faker Faker = new();

    public IntegrationFixture()
    {
        var application = new ApplicationFixture();

        Database = new DatabaseFixture(application);

        Client = new RequestFixture(application);

        Token = new TokenFixture(application);

        Arrange = new ArrangeFixture(application);

        Cache = application.GetService<IDistributedCache>();
    }

    public RequestFixture Client { get; }

    public IDistributedCache Cache { get; }

    public DatabaseFixture Database { get; }

    public ArrangeFixture Arrange { get; }

    public TokenFixture Token { get; }

    public Task InitializeAsync()
    {
        return Database.InitializeAsync();
    }

    public Task DisposeAsync()
    {
        return Database.DisposeAsync();
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}

public class ApplicationFixture : IAsyncLifetime, IDisposable
{
    public ApplicationFixture()
    {
        Key = new ECDsaSecurityKey(ECDsa.Create())
        {
            KeyId = "test"
        };
    }

    public Uri BaseAddress => Server.BaseAddress;

    public PostgreSqlFixture Default { get; } = new();

    public PostgreSqlFixture Cache { get; } = new();

    public TestServer Server { get; private set; } = null!;

    public IServiceProvider Services { get; private set; } = null!;

    public IWebHost Host { get; private set; } = null!;

    public SecurityKey Key { get; }

    public async Task InitializeAsync()
    {
        Host = CreateWebHost();

        await Default.InitializeAsync();

        await Cache.InitializeAsync();

        await Host.StartAsync();

        Services = Host.Services;

        Server = Host.GetTestServer();
    }

    public async Task DisposeAsync()
    {
        await Default.DisposeAsync();

        await Cache.DisposeAsync();

        await Host.StopAsync();
    }

    public void Dispose()
    {
        Host.Dispose();
    }

    public T GetService<T>() where T : notnull
    {
        return Services.GetRequiredService<T>();
    }

    private IWebHost CreateWebHost()
    {
        var builder = new WebHostBuilder().UseStartup<Program>().UseTestServer();

        ConfigureWebHost(builder);

        return builder.Build();
    }

    private void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting("ConnectionStrings:Default", Default.ConnectionString);
        builder.UseSetting("ConnectionStrings:Cache", Cache.ConnectionString);

        builder.ConfigureServices(services =>
        {
            services.AddDbContext<DbContext, AuthDbContext>();

            services.AddSingleton(Key);
        });
    }
}

public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlBuilder _builder = new();

    private readonly IDatabaseContainer _db;

    public PostgreSqlFixture()
    {
        _db = _builder.Build();
    }

    public string ConnectionString => _db.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _db.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
    }
}

public class RequestFixture(ApplicationFixture application) : IDisposable
{
    private readonly HttpClient _client = application.Server.CreateClient();

    private readonly JwtOptions options = application.GetService<IOptions<JwtOptions>>().Value;

    public void Dispose()
    {
        _client.Dispose();
    }

    public RequestBuilder Create(HttpMethod method, string uri)
    {
        return new RequestBuilder(_client, method, uri, options, application.Key);
    }

    public RequestBuilder Get(string uri)
    {
        return Create(HttpMethod.Get, uri);
    }

    public RequestBuilder Post(string uri, object payload)
    {
        return Create(HttpMethod.Post, uri).WithJson(payload);
    }

    public RequestBuilder Put(string uri, object payload)
    {
        return Create(HttpMethod.Put, uri).WithJson(payload);
    }

    public RequestBuilder Delete(string uri)
    {
        return Create(HttpMethod.Delete, uri);
    }
}

public class RequestBuilder(HttpClient client, HttpMethod method, string uri, JwtOptions options, SecurityKey key)
{
    private readonly HttpRequestMessage _request = new(method, uri);

    public RequestBuilder WithJson(object payload)
    {
        _request.Content = JsonContent.Create(payload);

        return this;
    }

    public RequestBuilder WithHeader(string name, string value)
    {
        _request.Headers.Add(name, value);

        return this;
    }

    public RequestBuilder WithAuthorization(string token)
    {
        _request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return this;
    }

    public RequestBuilder WithAuthorization(Action<AccessTokenBuilder> configure)
    {
        var builder = new AccessTokenBuilder(key, options);

        configure(builder);

        var token = builder.Build();

        return WithAuthorization(token);
    }

    public async Task<HttpResponseMessage> SendAsync()
    {
        return await client.SendAsync(_request);
    }
}

public class TokenFixture(ApplicationFixture application)
{
    private readonly JwtOptions _options = application.GetService<IOptions<JwtOptions>>().Value;

    public AccessTokenBuilder AccessToken => new(application.Key, _options);
}

public class AccessTokenBuilder(SecurityKey key, JwtOptions options)
{
    private readonly List<Claim> _claims = new();

    private readonly JwtSecurityTokenHandler _handler = new();

    public AccessTokenBuilder WithSession(Guid id)
    {
        _claims.Add(new Claim(JwtRegisteredClaimNames.Jti, id.ToString()));

        return this;
    }

    public AccessTokenBuilder WithUser(Guid id)
    {
        _claims.Add(new Claim(JwtRegisteredClaimNames.Sub, id.ToString()));

        return this;
    }

    public AccessTokenBuilder WithRoles(params string[] roles)
    {
        foreach (var role in roles)
        {
            _claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return this;
    }

    public string Build()
    {
        var signing = new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256);

        var token = new JwtSecurityToken(
            options.Issuer,
            options.Audience,
            _claims,
            DateTime.UtcNow,
            DateTime.UtcNow.Add(options.AccessTokenLifetime),
            signing
        );

        return _handler.WriteToken(token);
    }
}