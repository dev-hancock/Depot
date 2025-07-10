namespace Depot.Auth.Features.Users.Me;

using ErrorOr;
using Mestra.Abstractions;

public sealed record MeQuery : IRequest<ErrorOr<MeResponse>>;