namespace Depot.Auth.Features.Auth.Login;

using System.Text.Json.Serialization;

public class LoginResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = null!;
}