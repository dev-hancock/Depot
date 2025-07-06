namespace Depot.Common.Models;

using System.Text.Json.Serialization;

public class User
{
    [JsonPropertyName("username")]
    public string Username { get; init; } = null!;
}