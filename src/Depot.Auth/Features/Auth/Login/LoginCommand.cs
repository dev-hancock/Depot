using System.Text.Json.Serialization;
using ErrorOr;
using Mestra.Abstractions;

namespace Depot.Auth.Features.Auth.Login;

public class LoginCommand : IRequest<ErrorOr<LoginResponse>>
{
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; } = null!;
}