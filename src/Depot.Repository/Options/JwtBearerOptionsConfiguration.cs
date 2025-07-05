namespace Depot.Repository.Options;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Services;

public class JwtBearerOptionsConfiguration : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwt;

    private readonly ISecurityKeyProvider _key;

    public JwtBearerOptionsConfiguration(IOptions<JwtOptions> options, ISecurityKeyProvider key)
    {
        _jwt = options.Value;
        _key = key;
    }

    public void Configure(JwtBearerOptions options)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key.GetSecurityKey(_jwt.KeyPath)
        };
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }
}