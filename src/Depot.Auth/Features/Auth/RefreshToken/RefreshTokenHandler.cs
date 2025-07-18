namespace Depot.Auth.Features.Auth.RefreshToken;

using System.Reactive.Linq;
using Domain.Auth;
using Domain.Errors;
using Domain.Interfaces;
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
        await using var context = await _factory.CreateDbContextAsync(token);

        var user = await context.Users
            .Include(x => x.Sessions)
            .ThenInclude(x => x.RefreshToken)
            .Where(x => x.Id == _user.UserId)
            .SingleOrDefaultAsync(token);

        if (user is null)
        {
            return Errors.UserNotFound();
        }

        var session = user.FindSession(message.RefreshToken);

        if (session is null)
        {
            return Error.Unauthorized();
        }

        var now = _time.UtcNow;

        session.Refresh(_tokens.GenerateRefreshToken(now));

        await context.SaveChangesAsync(token);

        return new RefreshTokenResponse
        {
            AccessToken = _tokens.GenerateAccessToken(user, now),
            RefreshToken = session.RefreshToken
        };
    }
}