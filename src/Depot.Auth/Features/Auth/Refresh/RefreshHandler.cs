namespace Depot.Auth.Features.Auth.Refresh;

using System.Reactive.Linq;
using Domain.Auth;
using Domain.Interfaces;
using Domain.Users.Errors;
using ErrorOr;
using Mappings;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Middleware;
using Persistence;

public class RefreshHandler : IMessageHandler<RefreshCommand, ErrorOr<RefreshResponse>>
{
    private readonly AuthDbContext _context;

    private readonly ITimeProvider _time;

    private readonly ITokenGenerator _tokens;

    private readonly IUserContext _user;

    public RefreshHandler(
        AuthDbContext context,
        IUserContext user,
        ITimeProvider time,
        ITokenGenerator tokens)
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

        var result = user.RefreshSession(message.RefreshToken, _tokens.GenerateRefreshToken(now).ToRefreshToken(), now);

        if (result.Value is not { } session)
        {
            return ErrorOr<RefreshResponse>.From(result.Errors);
        }

        await _context.SaveChangesAsync(token);

        return new RefreshResponse
        {
            AccessToken = _tokens
                .GenerateAccessToken(
                    user.Id.Value,
                    session.Id.Value,
                    [],
                    now)
                .ToAccessToken(),
            RefreshToken = session.RefreshToken
        };
    }
}