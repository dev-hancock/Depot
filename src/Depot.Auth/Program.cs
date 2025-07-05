namespace Depot.Auth;

using Domain;
using Endpoints;
using Extensions;
using Mestra.Extensions.Microsoft.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Middleware;
using Persistence;
using Scalar.AspNetCore;
using Serilog;
using Services;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                "/var/log/depot/auth-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14)
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog();

        ConfigureServices(builder);

        var app = builder.Build();

        Configure(app);

        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.AddJwtAuthentication();

        var services = builder.Services;

        services.AddOpenApi();

        services.AddMestra(opt => opt.AddHandlersFromAssembly(typeof(Program).Assembly));

        services.AddDbContextFactory<AuthDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

        services.AddSingleton<ISecureRandom, SecureRandom>();
        services.AddSingleton<ISecretHasher, SecretHasher>();

        services.AddSingleton<ITokenGenerator, TokenGenerator>();

        services.AddSingleton(TimeProvider.System);

        services.AddAuthorization();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
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
}