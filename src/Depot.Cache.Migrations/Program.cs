namespace Depot.Cache.Migrations;

using Sloop.Abstractions;
using Sloop.Extensions;

public class Program
{
    public async static Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddCache(opt =>
        {
            opt.UseConnectionString(configuration.GetConnectionString("Cache")!);
            opt.CreateInfrastructure = true;
        });

        var app = builder.Build();

        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<IDbCacheContext>();

        try
        {
            await context.MigrateAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}