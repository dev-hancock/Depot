using ErrorOr;
using Mestra.Abstractions;

namespace Depot.Auth.Features.Users.Me;

public sealed record MeQuery : IRequest<ErrorOr<MeResponse>>;