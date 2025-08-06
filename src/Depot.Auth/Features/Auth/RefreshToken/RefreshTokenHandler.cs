namespace Depot.Auth.Features.Auth.RefreshToken;

using System.Reactive.Linq;
using Domain.Auth;
using Domain.Interfaces;
using Domain.Users.Errors;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Middleware;
using Persistence;

public class RefreshTokenHandler : IMessageHandler<RefreshTokenCommand, ErrorOr<RefreshTokenResponse>>
{
    private readonly IDbContextFactory<AuthDbContext> _factory;

    private readonly ITimeProvider _time;

    private readonly ITokenGenerator _tokens;

    private readonly IUserContext _user;

    public RefreshTokenHandler(
        IDbContextFactory<AuthDbContext> factory,
        IUserContext user,
        ITimeProvider time,
        ITokenGenerator tokens)
    {
        _factory = factory;
        _user = user;
        _time = time;
        _tokens = tokens;
    }

    public IObservable<ErrorOr<RefreshTokenResponse>> Handle(RefreshTokenCommand message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<RefreshTokenResponse>> Handle(RefreshTokenCommand message, CancellationToken token)
    {
        await using var db = await _factory.CreateDbContextAsync(token);

        var user = await db.Users
            .Include(x => x.Sessions)
            .ThenInclude(x => x.RefreshToken)
            .Where(x => x.Id == new UserId(_user.UserId))
            .SingleOrDefaultAsync(token);

        if (user is null)
        {
            return Errors.UserNotFound();
        }

        var now = _time.UtcNow;

        var result = user.RefreshSession(message.RefreshToken, _tokens.GenerateRefreshToken(now), now);

        if (result.Value is not { } session)
        {
            return ErrorOr<RefreshTokenResponse>.From(result.Errors);
        }

        await db.SaveChangesAsync(token);

        return new RefreshTokenResponse
        {
            AccessToken = _tokens.GenerateAccessToken(user, session.Id, now),
            RefreshToken = session.RefreshToken
        };
    }
}