namespace Depot.Repository.Extensions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Options;
using Services;

public static class JwtExtensions
{
    public static WebApplicationBuilder AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        builder.Services
            .AddOptions<JwtOptions>()
            .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
            .Validate(o =>
            {
                o.Validate();

                return true;
            })
            .ValidateOnStart();

        builder.Services.AddSingleton<ISecurityKeyProvider>(sp =>
        {
            return new SecurityKeyProvider(c)
        });

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        builder.Services.ConfigureOptions<JwtBearerOptionsConfiguration>();

        return builder;
    }
}