using System.Reactive.Linq;
using Depot.Auth.Domain.Auth;
using Depot.Auth.Domain.Users;
using Depot.Auth.Domain.Users.Errors;
using Depot.Auth.Middleware;
using Depot.Auth.Persistence;
using Depot.Auth.Services;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Depot.Auth.Features.Users.ChangePassword;

public class ChangePasswordHandler : IMessageHandler<ChangePasswordCommand, ErrorOr<ChangePasswordResponse>>
{
    private readonly AuthDbContext _context;

    private readonly ISecretHasher _hasher;

    private readonly ITimeProvider _time;

    private readonly ITokenService _tokens;

    private readonly IUserContext _user;

    public ChangePasswordHandler(
        AuthDbContext context,
        IUserContext user,
        ISecretHasher hasher,
        ITokenService tokens,
        ITimeProvider time)
    {
        _context = context;
        _user = user;
        _hasher = hasher;
        _tokens = tokens;
        _time = time;
    }

    public IObservable<ErrorOr<ChangePasswordResponse>> Handle(ChangePasswordCommand message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<ChangePasswordResponse>> Handle(ChangePasswordCommand message, CancellationToken token)
    {
        var user = await _context.Users
            .Where(x => x.Id == new UserId(_user.UserId))
            .Include(x => x.Sessions)
            .SingleOrDefaultAsync(token);

        if (user is null)
        {
            return Errors.UserNotFound();
        }

        if (!_hasher.Verify(user.Password, message.OldPassword))
        {
            return Error.Unauthorized();
        }

        user.ChangePassword(Password.Create(_hasher.Hash(message.NewPassword)));

        user.RevokeSession(_time.UtcNow);

        var now = _time.UtcNow;

        var session = user.CreateSession(_tokens.GetRefreshToken(now));

        await _context.SaveChangesAsync(token);

        return new ChangePasswordResponse
        {
            AccessToken = _tokens.GetAccessToken(user, session, now),
            RefreshToken = session.RefreshToken
        };
    }
}