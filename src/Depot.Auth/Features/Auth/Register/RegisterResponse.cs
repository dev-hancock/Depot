using System.Text.Json.Serialization;

namespace Depot.Auth.Features.Auth.Register;

public class RegisterResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = null!;
}