namespace Depot.Auth.Features.Auth.Login;

using System.Text.Json.Serialization;
using ErrorOr;
using Mestra.Abstractions;

public class LoginCommand : IRequest<ErrorOr<LoginResponse>>
{
    [JsonPropertyName("username")]
    public string? Username { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("password")]
    public string Password { get; init; } = null!;
}