using Depot.Repository.Endpoints;
using Depot.Repository.Extensions;
using Depot.Repository.Middleware;
using Depot.Repository.Persistence;
using Depot.Storage;
using Mestra.Extensions.Microsoft.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

namespace Depot.Repository;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                "/var/log/depot/repo-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14)
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog();

        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        Configure(app);

        app.Run();
    }

    private static void Configure(WebApplication app)
    {
        app.MapOpenApi();

        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference();
        }

        app.UseExceptionHandler();

        app.UseRouting();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapEndpoints();
        app.MapGet("/ping", () => "pong");
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddJwtAuthentication(opt => configuration.GetSection("Jwt").Bind(opt));

        services.AddOpenApi();

        services.AddMestra(opt => opt.AddHandlersFromAssembly(typeof(Program).Assembly));

        services.AddDbContextFactory<RepoDbContext>(opt => opt.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddStorage(configuration);

        services.AddSingleton(TimeProvider.System);

        services.AddAuthorization();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }
}