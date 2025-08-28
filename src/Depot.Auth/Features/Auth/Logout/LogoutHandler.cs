namespace Depot.Auth.Features.Auth.Logout;

using System.Reactive.Linq;
using Domain.Auth;
using Domain.Interfaces;
using Domain.Users.Errors;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Middleware;
using Persistence;

public class LogoutHandler : IMessageHandler<LogoutCommand, ErrorOr<Success>>
{
    private readonly AuthDbContext _context;

    private readonly ISecretHasher _hasher;

    private readonly TimeProvider _time;

    private readonly IUserContext _user;

    public LogoutHandler(AuthDbContext context, IUserContext user, ISecretHasher hasher, TimeProvider time)
    {
        _context = context;
        _user = user;
        _hasher = hasher;
        _time = time;
    }

    public IObservable<ErrorOr<Success>> Handle(LogoutCommand message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<Success>> Handle(LogoutCommand message, CancellationToken token)
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

        var result = user.RevokeSession(message.RefreshToken);

        if (result.IsError)
        {
            return result;
        }

        await _context.SaveChangesAsync(token);

        return Result.Success;
    }
}