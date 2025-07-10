namespace Depot.Auth.Features.Auth.Logout;

using System.Text.Json.Serialization;
using ErrorOr;
using Mestra.Abstractions;

public sealed class LogoutCommand : IRequest<ErrorOr<Success>>
{
    [JsonPropertyName("refresh_token")]
    public string? Token { get; set; }
}