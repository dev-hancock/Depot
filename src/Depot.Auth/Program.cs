using Asp.Versioning;
using Depot.Auth.Endpoints;
using Depot.Auth.Extensions;
using Depot.Auth.Middleware;
using Depot.Auth.Middleware.Exceptions;
using Depot.Auth.Persistence;
using Depot.Auth.Services;
using Depot.Auth.Validation;
using FluentValidation;
using Mestra.Abstractions;
using Mestra.Extensions.Microsoft.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Sloop.Extensions;

namespace Depot.Auth;

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
            app.MapScalarApiReference(opt => opt.AddDocuments("v1"));
        }

        app.UseExceptionHandler();
        app.UseRouting();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseMiddleware<UserContextMiddleware>();
        app.UseStatusCodePages();
        app.UseAuthorization();
        app.MapEndpoints();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddJwtAuthentication(opt => configuration.GetSection("Jwt").Bind(opt));

        services.AddOpenApi("v1");

        services
            .AddApiVersioning(opt =>
            {
                opt.ReportApiVersions = true;
                opt.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(opt =>
            {
                opt.GroupNameFormat = "'v'V";
                opt.SubstituteApiVersionInUrl = true;
            });

        // services.AddSerilog(Log.Logger);

        services.AddMestra(opt =>
        {
            opt.Lifetime = ServiceLifetime.Scoped;
            opt.AddHandlersFromAssembly(typeof(Program).Assembly);
        });
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddCache(opt =>
        {
            opt.UseConnectionString(configuration.GetConnectionString("Cache")!);
            opt.CreateInfrastructure = true;
        });

        services.AddScoped<DomainEventsInterceptor>();

        services.AddDbContext<AuthDbContext>((sp, opt) =>
        {
            opt.UseNpgsql(configuration.GetConnectionString("Default")!)
                .AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
        });

        services.AddSingleton<ISecureRandom, SecureRandom>();
        services.AddSingleton<ISecretHasher, SecretHasher>();

        services.AddSingleton<ITokenService, TokenService>();

        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ITenantContext, TenantContext>();

        services.AddSingleton<ITimeProvider, SystemTimeProvider>();

        services.AddAuthorization();
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }
}