using System.Text.Json.Serialization;

namespace Depot.Auth.Features.Users.ChangePassword;

public class ChangePasswordResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = null!;
}