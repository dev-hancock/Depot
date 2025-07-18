namespace Depot.Auth.Features.Auth.Logout;

using System.Reactive.Linq;
using Domain.Errors;
using Domain.Interfaces;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Middleware;
using Persistence;

public class LogoutHandler : IMessageHandler<LogoutCommand, ErrorOr<Success>>
{
    private readonly IDbContextFactory<AuthDbContext> _factory;

    private readonly ISecretHasher _hasher;

    private readonly TimeProvider _time;

    private readonly IUserContext _user;

    public LogoutHandler(IDbContextFactory<AuthDbContext> factory, IUserContext user, ISecretHasher hasher, TimeProvider time)
    {
        _factory = factory;
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

        var result = user.Logout(message.Token);

        if (result.IsError)
        {
            return result;
        }

        await context.SaveChangesAsync(token);

        return Result.Success;
    }
}