namespace Depot.Auth.Services;

using Microsoft.Extensions.Options;

public class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            return ValidateOptionsResult.Fail("Issuer is not set");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            return ValidateOptionsResult.Fail("Audience is not set");
        }

        if (options.AccessTokenLifetime <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail("AccessTokenLifetime must be positive");
        }

        if (options.RefreshTokenLifetime <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail("RefreshTokenLifetime must be positive");
        }

        return ValidateOptionsResult.Success;
    }
}