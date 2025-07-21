namespace Depot.Auth.Migrator;

using Microsoft.EntityFrameworkCore;
using Persistence;

public class Program
{
    public async static Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        var configuration = builder.Configuration;

        builder.Services.AddDbContextFactory<AuthDbContext>(opt =>
        {
            opt.UseNpgsql(
                configuration.GetConnectionString("Auth"),
                x => x
                    .MigrationsAssembly("Depot.Auth.Migrations")
                    .MigrationsHistoryTable("__EFMigrationsHistory", "auth"));
        });

        var app = builder.Build();

        using var scope = app.Services.CreateScope();

        await using var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}