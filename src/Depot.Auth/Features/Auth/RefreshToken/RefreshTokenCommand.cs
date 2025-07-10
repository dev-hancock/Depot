namespace Depot.Auth.Features.Auth.RefreshToken;

using System.Text.Json.Serialization;
using ErrorOr;
using Mestra.Abstractions;

public sealed class RefreshTokenCommand : IRequest<ErrorOr<RefreshTokenResponse>>
{
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; } = null!;
}