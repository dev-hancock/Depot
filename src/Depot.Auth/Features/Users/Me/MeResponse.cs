using System.Text.Json.Serialization;

namespace Depot.Auth.Features.Users.Me;

public class MeResponse
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;
}