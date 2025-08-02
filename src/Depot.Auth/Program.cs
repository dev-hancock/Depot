namespace Depot.Auth;

using Domain.Interfaces;
using Endpoints;
using Extensions;
using Mestra.Extensions.Microsoft.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Middleware;
using Persistence;
using Scalar.AspNetCore;
using Services;
using Sloop.Extensions;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Log.Logger = new LoggerConfiguration()
        //     .ReadFrom.Configuration(builder.Configuration)
        //     .MinimumLevel.Information()
        //     .Enrich.FromLogContext()
        //     .WriteTo.Console()
        //     .WriteTo.File(
        //         "/var/log/depot/auth-.log",
        //         rollingInterval: RollingInterval.Day,
        //         retainedFileCountLimit: 14)
        //     .CreateLogger();

        ConfigureServices(builder);

        var app = builder.Build();

        Configure(app);

        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.AddJwtAuthentication();

        var services = builder.Services;
        var configuration = builder.Configuration;

        // services.AddSerilog(Log.Logger);
        services.AddOpenApi();

        services.AddMestra(opt =>
        {
            opt.Lifetime = ServiceLifetime.Scoped;
            opt.AddHandlersFromAssembly(typeof(Program).Assembly);
        });
        services.AddCache(opt =>
        {
            opt.UseConnectionString(configuration.GetConnectionString("Cache")!);
            opt.CreateInfrastructure = true;
        });

        services.AddEntityFrameworkNpgsql();
        services.AddDbContextFactory<AuthDbContext>((sp, opt) =>
        {
            opt.UseInternalServiceProvider(sp);
            opt.UseNpgsql(configuration.GetConnectionString("Auth"));
        });

        services.AddSingleton<ISecureRandom, SecureRandom>();
        services.AddSingleton<ISecretHasher, SecretHasher>();

        services.AddSingleton<ITokenGenerator, TokenGenerator>();

        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ITenantContext, TenantContext>();

        services.AddSingleton<ITimeProvider, SystemTimeProvider>();

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
        app.UseMiddleware<UserContextMiddleware>();
        app.UseAuthorization();

        app.MapEndpoints();
        app.MapGet("/ping", () => "pong");
    }
}