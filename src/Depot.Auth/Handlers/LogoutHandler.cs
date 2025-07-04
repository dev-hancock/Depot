namespace Depot.Auth.Handlers;

using System.Reactive;
using System.Reactive.Linq;
using Domain;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Persistence;

public class LogoutHandler : IMessageHandler<LogoutHandler.Request, ErrorOr<Unit>>
{
    private readonly IDbContextFactory<AuthDbContext> _factory;

    private readonly ISecretHasher _hasher;

    private readonly TimeProvider _time;

    public LogoutHandler(IDbContextFactory<AuthDbContext> factory, ISecretHasher hasher, TimeProvider time)
    {
        _factory = factory;
        _hasher = hasher;
        _time = time;
    }

    public IObservable<ErrorOr<Unit>> Handle(Request message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<Unit>> Handle(Request message, CancellationToken token)
    {
        await using var context = await _factory.CreateDbContextAsync(token);

        var user = await context.Users
            .Include(x => x.Tokens)
            .Where(x => x.Id == message.UserId)
            .SingleOrDefaultAsync(token);

        if (user is null)
        {
            return Errors.UserNotFound();
        }

        if (message.Token is null)
        {
            user.ClearSessions();
        }
        else
        {
            var result = RefreshToken.Parse(message.Token).Then(x => user.RevokeSession(x, _hasher, _time));

            if (result.IsError)
            {
                return result;
            }
        }

        await context.SaveChangesAsync(token);

        return Unit.Default;
    }

    public sealed record Request(Guid UserId, string? Token = null) : IRequest<ErrorOr<Unit>>;
}