namespace Depot.Auth.Features.Organisations.CreateOrganisation;

using System.Reactive.Linq;
using Domain.Auth;
using Domain.Organisations;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Persistence;

public class CreateOrganisationHandler : IMessageHandler<CreateOrganisationHandler.Request, ErrorOr<Organisation>>
{
    private readonly IDbContextFactory<AuthDbContext> _factory;

    private readonly TimeProvider _time;

    public CreateOrganisationHandler(IDbContextFactory<AuthDbContext> factory, TimeProvider time)
    {
        _factory = factory;
        _time = time;
    }

    public IObservable<ErrorOr<Organisation>> Handle(Request message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<Organisation>> Handle(Request message, CancellationToken token)
    {
        await using var context = await _factory.CreateDbContextAsync(token);

        var user = await context.Users
            .AsNoTracking()
            .Where(x => x.Id == new UserId(message.UserId))
            .FirstOrDefaultAsync(token);

        if (user is null)
        {
            return Error.NotFound();
        }

        if (!user.CanCreateOrganisation())
        {
            return Error.Unauthorized();
        }

        var exists = await context.Organisations
            .AsNoTracking()
            .Where(x => x.Slug == message.Name)
            .Where(x => x.CreatedBy == user.Id)
            .AnyAsync(token);

        if (exists)
        {
            return Error.Conflict();
        }

        var result = Organisation.New(message.Name, user.Id, _time);

        if (result.IsError)
        {
            return result;
        }

        context.Organisations.Add(result.Value);

        await context.SaveChangesAsync(token);

        return result;
    }

    public sealed record Request(Guid UserId, string Name) : IRequest<ErrorOr<Organisation>>;
}