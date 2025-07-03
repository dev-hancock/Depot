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

    public LogoutHandler(IDbContextFactory<AuthDbContext> factory)
    {
        _factory = factory;
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
            user.RevokeTokens();
        }
        else
        {
            var result = RefreshToken.Parse(message.Token).Then(x => user.RevokeToken(x));

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