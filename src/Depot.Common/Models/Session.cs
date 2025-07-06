namespace Depot.Common.Models;

using System.Text.Json.Serialization;

public class Session
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = null!;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = null!;
}