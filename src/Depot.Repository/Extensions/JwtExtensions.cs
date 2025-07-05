namespace Depot.Repository.Extensions;

using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Services;

public static class JwtExtensions
{
    public static WebApplicationBuilder AddJwtAuthentication(this WebApplicationBuilder builder)
    {
        var options = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

        if (options == null)
        {
            throw new NullReferenceException("JWT options not found");
        }

        if (string.IsNullOrWhiteSpace(options.KeyPath))
        {
            throw new NullReferenceException("JWT key path is required");
        }

        var filepath = options.KeyPath;

        if (!Path.IsPathRooted(filepath))
        {
            filepath = Path.Combine(Directory.GetCurrentDirectory(), filepath);
        }

        if (!File.Exists(filepath))
        {
            throw new FileNotFoundException("JWT key file not found.", filepath);
        }

        var ecdsa = ECDsa.Create();

        ecdsa.ImportFromPem(File.ReadAllText(filepath));

        var key = new ECDsaSecurityKey(ecdsa)
        {
            KeyId = "depot-key"
        };

        builder.Services.AddSingleton(key);

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,
                    ValidateAudience = true,
                    ValidAudience = options.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key
                };
            });

        return builder;
    }
}