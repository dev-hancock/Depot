using Depot.Auth.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Depot.Auth.Migrator;

public class Program
{
    public static async Task Main(string[] args)
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