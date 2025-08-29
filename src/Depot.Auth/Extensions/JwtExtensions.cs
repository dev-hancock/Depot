using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Depot.Auth.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, Action<JwtOptions> configure)
    {
        services.Configure(configure);

        services.AddSingleton<SecurityKey>(sp =>
        {
            var jwt = sp.GetRequiredService<IOptions<JwtOptions>>().Value;

            var ecdsa = ECDsa.Create();

            if (File.Exists(jwt.KeyPath))
            {
                ecdsa.ImportFromPem(File.ReadAllText(jwt.KeyPath));
            }

            return new ECDsaSecurityKey(ecdsa)
            {
                KeyId = "depot-key"
            };
        });

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IServiceProvider>((opt, sp) =>
            {
                var key = sp.GetRequiredService<SecurityKey>();
                var jwt = sp.GetRequiredService<IOptions<JwtOptions>>().Value;

                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key
                };
            });

        return services;
    }
}