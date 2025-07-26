namespace Depot.Auth.Features.Auth.Logout;

using System.Reactive.Linq;
using Domain.Auth;
using Domain.Errors;
using Domain.Interfaces;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Middleware;
using Persistence;

public class LogoutHandler : IMessageHandler<LogoutCommand, ErrorOr<Success>>
{
    private readonly IUserContext _context;

    private readonly IDbContextFactory<AuthDbContext> _factory;

    private readonly ISecretHasher _hasher;

    private readonly TimeProvider _time;

    public LogoutHandler(IDbContextFactory<AuthDbContext> factory, IUserContext context, ISecretHasher hasher, TimeProvider time)
    {
        _factory = factory;
        _context = context;
        _hasher = hasher;
        _time = time;
    }

    public IObservable<ErrorOr<Success>> Handle(LogoutCommand message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<Success>> Handle(LogoutCommand message, CancellationToken token)
    {
        await using var db = await _factory.CreateDbContextAsync(token);

        var user = await db.Users
            .Include(x => x.Sessions)
            .ThenInclude(x => x.RefreshToken)
            .Where(x => x.Id == new UserId(_context.UserId))
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

        await db.SaveChangesAsync(token);

        return Result.Success;
    }
}