namespace Depot.Repository.Migrations;

using Microsoft.EntityFrameworkCore;
using Persistence;

public class Program
{
    public async static Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        var configuration = builder.Configuration;

        builder.Services.AddDbContextFactory<RepoDbContext>(opt =>
        {
            opt.UseNpgsql(
                configuration.GetConnectionString("Default"),
                x => x
                    .MigrationsAssembly("Depot.Repository.Migrations")
                    .MigrationsHistoryTable("__EFMigrationsHistory", "repo"));
        });

        var app = builder.Build();

        using var scope = app.Services.CreateScope();

        await using var context = scope.ServiceProvider.GetRequiredService<RepoDbContext>();

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