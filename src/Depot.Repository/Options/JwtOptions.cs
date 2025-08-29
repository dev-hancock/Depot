using System.ComponentModel.DataAnnotations;

namespace Depot.Repository.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = null!;

    [Required]
    public string Audience { get; set; } = null!;

    [Required]
    public string KeyPath { get; set; } = null!;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(KeyPath))
        {
            throw new NullReferenceException("JWT key path is required");
        }

        if (!File.Exists(KeyPath))
        {
            throw new FileNotFoundException("JWT key file not found.", KeyPath);
        }

        if (string.IsNullOrWhiteSpace(Issuer))
        {
            throw new NullReferenceException("JWT issuer is required");
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            throw new NullReferenceException("JWT audience is required");
        }

        if (Issuer == Audience)
        {
            throw new ArgumentException("JWT issuer and audience must both be the same");
        }
    }
}