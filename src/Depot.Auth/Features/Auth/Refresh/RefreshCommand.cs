namespace Depot.Auth.Features.Auth.Refresh;

using System.Text.Json.Serialization;
using ErrorOr;
using Mestra.Abstractions;

public sealed class RefreshCommand : IRequest<ErrorOr<RefreshResponse>>
{
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = null!;
}