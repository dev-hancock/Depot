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
            .Include(x => x.Tokens)
            .Where(x => x.Id == _user.UserId)
            .SingleOrDefaultAsync(token);

        if (user is null)
        {
            return Errors.UserNotFound();
        }

        if (string.IsNullOrWhiteSpace(message.Token))
        {
            _ = user.ClearSessions();
        }
        else
        {
            var result = RefreshToken
                .Parse(message.Token)
                .Then(x => user
                    .RevokeSession(x, _hasher, _time));

            if (result.IsError)
            {
                return ErrorOr<Success>.From(result.Errors);
            }
        }

        await context.SaveChangesAsync(token);

        return Result.Success;
    }
}