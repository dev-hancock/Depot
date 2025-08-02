namespace Depot.Auth.Features.Users.ChangePassword;

using System.Text.Json.Serialization;

public class ChangePasswordResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = null!;

    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }
}