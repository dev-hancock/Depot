namespace Depot.Auth.Tests.Fixtures.Application;

using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using TUnit.Core.Interfaces;

public class ApplicationFixture : WebApplicationFactory<Program>, IAsyncInitializer
{
    public SecurityKey Key = new ECDsaSecurityKey(ECDsa.Create());

    [ClassDataSource<DefaultDatabaseFixture>(Shared = SharedType.PerTestSession)]
    public required DefaultDatabaseFixture Default { get; set; } = null!;

    [ClassDataSource<CacheDatabaseFixture>(Shared = SharedType.PerTestSession)]
    public required CacheDatabaseFixture Cache { get; set; } = null!;

    public Uri BaseAddress => Server.BaseAddress;

    public HttpClient Client { get; set; } = null!;

    public Task InitializeAsync()
    {
        Client = CreateClient();

        return Task.CompletedTask;
    }

    public T GetService<T>() where T : notnull
    {
        return Services.GetRequiredService<T>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting("ConnectionStrings:Default", Default.ConnectionString);
        builder.UseSetting("ConnectionStrings:Cache", Cache.ConnectionString);

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(Key);
            services.AddDbContextFactory<AuthDbContext>(opt =>
            {
                opt.UseNpgsql(Default.ConnectionString);
            });
        });
    }
}