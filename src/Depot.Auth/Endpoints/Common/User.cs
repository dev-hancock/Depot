namespace Depot.Auth.Endpoints.Common;

using System.Text.Json.Serialization;

public class User
{
    [JsonPropertyName("username")]
    public string Username { get; init; } = null!;
}

public class Session
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = null!;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = null!;
}