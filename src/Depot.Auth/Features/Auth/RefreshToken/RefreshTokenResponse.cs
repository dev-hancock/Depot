namespace Depot.Auth.Features.Auth.RefreshToken;

using System.Text.Json.Serialization;

public sealed class RefreshTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = null!;

    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }
}