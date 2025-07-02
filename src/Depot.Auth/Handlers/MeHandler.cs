namespace Depot.Auth.Handlers;

using System.Reactive.Linq;
using Domain;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Persistence;

public class MeHandler : IMessageHandler<MeHandler.Request, ErrorOr<User>>
{
    private readonly IDbContextFactory<AuthDbContext> _factory;

    public MeHandler(IDbContextFactory<AuthDbContext> factory)
    {
        _factory = factory;
    }

    public IObservable<ErrorOr<User>> Handle(Request message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<User>> Handle(Request message, CancellationToken token)
    {
        await using var context = await _factory.CreateDbContextAsync(token);

        var user = await context.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Where(x => x.Id.Equals(message.UserId))
            .SingleOrDefaultAsync(token);

        if (user is null)
        {
            return Errors.UserNotFound();
        }

        return user;
    }

    public record Request(Guid UserId) : IRequest<ErrorOr<User>>;
}