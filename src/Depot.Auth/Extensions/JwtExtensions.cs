using System.Security.Cryptography;
using Depot.Auth.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Depot.Auth.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, Action<JwtOptions> configure)
    {
        var jwt = new JwtOptions();

        configure(jwt);

        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(jwt));

        var ecdsa = ECDsa.Create();

        if (File.Exists(jwt.KeyPath))
        {
            ecdsa.ImportFromPem(File.ReadAllText(jwt.KeyPath));
        }

        var key = new ECDsaSecurityKey(ecdsa)
        {
            KeyId = "depot-key"
        };

        services.AddSingleton<SecurityKey>(key);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
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