using System.Text.Json.Serialization;

namespace Depot.Common.Models;

public class Session
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = null!;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = null!;
}