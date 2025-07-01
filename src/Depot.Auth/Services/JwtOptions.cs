namespace Depot.Auth.Services;

using System.ComponentModel.DataAnnotations;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = null!;

    [Required]
    public string Audience { get; set; } = null!;

    [Required]
    public TimeSpan AccessTokenLifetime { get; set; }

    [Required]
    public TimeSpan RefreshTokenLifetime { get; set; }
}