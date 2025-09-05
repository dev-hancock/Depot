using System.Reactive.Linq;
using Depot.Auth.Domain.Auth;
using Depot.Auth.Domain.Users.Errors;
using Depot.Auth.Middleware;
using Depot.Auth.Persistence;
using Depot.Auth.Services;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Depot.Auth.Features.Auth.Refresh;

public class RefreshHandler : IMessageHandler<RefreshCommand, ErrorOr<RefreshResponse>>
{
    private readonly AuthDbContext _context;

    private readonly ITimeProvider _time;

    private readonly ITokenService _tokens;

    private readonly IUserContext _user;

    public RefreshHandler(
        AuthDbContext context,
        IUserContext user,
        ITimeProvider time,
        ITokenService tokens)
    {
        _context = context;
        _user = user;
        _time = time;
        _tokens = tokens;
    }

    public IObservable<ErrorOr<RefreshResponse>> Handle(RefreshCommand message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<RefreshResponse>> Handle(RefreshCommand message, CancellationToken token)
    {
        var user = await _context.Users
            .Include(x => x.Sessions)
            .ThenInclude(x => x.RefreshToken)
            .Where(x => x.Id == new UserId(_user.UserId))
            .SingleOrDefaultAsync(token);

        if (user is null)
        {
            return Errors.UserNotFound();
        }

        var now = _time.UtcNow;

        var result = user.RefreshSession(message.RefreshToken, _tokens.GetRefreshToken(now), now);

        if (result.Value is not { } session)
        {
            return Error.Unauthorized();
        }

        await _context.SaveChangesAsync(token);

        return new RefreshResponse
        {
            AccessToken = _tokens.GetAccessToken(user, session, now),
            RefreshToken = session.RefreshToken
        };
    }
}