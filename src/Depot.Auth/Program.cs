namespace Depot.Auth;

using System.Security.Cryptography;
using Domain;
using Endpoints;
using Mestra.Abstractions;
using Mestra.Extensions.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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

        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        Configure(app);

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();

        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateOnStart();

        services.AddMestra(opt => { opt.AddHandlersFromAssembly(typeof(Program).Assembly); });

        // TEMP
        services.AddTransient<IMessageHandlerFactory, MessageHandlerFactory>();
        services.AddTransient<IPipelineFactory, PipelineFactory>();

        services.AddDbContextFactory<AuthDbContext>(opt => opt.UseNpgsql(configuration.GetConnectionString("Default")));

        // services.AddScoped<IUserService, UserService>();
        // services.AddScoped<ITokenService, TokenService>();

        services.AddSingleton<ISecureRandom, SecureRandom>();
        services.AddSingleton<ISecretHasher, SecretHasher>();

        services.AddSingleton<ITokenGenerator, TokenGenerator>();

        services.AddSingleton(TimeProvider.System);

        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var key = new ECDsaSecurityKey(ecdsa)
        {
            KeyId = Guid.NewGuid().ToString()
        };

        services.AddSingleton(key);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key
                };
            });
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