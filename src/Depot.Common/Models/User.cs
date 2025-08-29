using System.Text.Json.Serialization;

namespace Depot.Common.Models;

public class User
{
    [JsonPropertyName("username")]
    public string Username { get; init; } = null!;
}