namespace Depot.Auth.Endpoints.Common;

using System.Text.Json.Serialization;

public class User
{
    [JsonPropertyName("username")]
    public string Username { get; init; } = null!;
}