namespace Depot.Auth.Features.Users.Me;

using System.Text.Json.Serialization;

public class MeResponse
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;
}