using System.ComponentModel.DataAnnotations;

namespace Depot.Auth.Extensions;

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

    [Required]
    public string KeyPath { get; set; } = null!;
}