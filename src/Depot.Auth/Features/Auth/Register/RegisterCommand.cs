using System.Text.Json.Serialization;
using ErrorOr;
using Mestra.Abstractions;

namespace Depot.Auth.Features.Auth.Register;

public class RegisterCommand : IRequest<ErrorOr<RegisterResponse>>
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;

    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    [JsonPropertyName("password")]
    public string Password { get; set; } = null!;
}