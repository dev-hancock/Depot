namespace Depot.Auth.Tests.Setup;

using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;

public static class Global
{
    public static readonly PostgreSqlContainer Auth = new PostgreSqlBuilder()
        .WithDatabase("auth")
        .Build();

    public static readonly PostgreSqlContainer Cache = new PostgreSqlBuilder()
        .WithDatabase("cache")
        .Build();

    public static readonly Application Application = new();

    public static readonly SecurityKey Key = new ECDsaSecurityKey(ECDsa.Create());

    public static HttpClient Client = null!;

    public static T Service<T>() where T : notnull
    {
        return Application.Services.GetRequiredService<T>();
    }

    [Before(TestSession)]
    public async static Task Setup()
    {
        await Auth.StartAsync();
        await Cache.StartAsync();

        Client = Application.CreateClient();

        await Database.Setup();
    }

    [After(TestSession)]
    public async static Task Teardown()
    {
        await Auth.DisposeAsync();
        await Cache.DisposeAsync();

        await Application.DisposeAsync();

        await Database.Teardown();
    }
}