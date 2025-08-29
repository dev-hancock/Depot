using System.Text.Json.Serialization;
using ErrorOr;
using Mestra.Abstractions;

namespace Depot.Auth.Features.Auth.Logout;

public sealed class LogoutCommand : IRequest<ErrorOr<Success>>
{
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
}