namespace Depot.Auth.Handlers.Organisations;

using System.Reactive.Linq;
using Domain.Common;
using Domain.Tenants;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Persistence;

public class CreateTenantHandler : IMessageHandler<CreateTenantHandler.Request, ErrorOr<Tenant>>
{
    private readonly IDbContextFactory<AuthDbContext> _factory;

    private readonly TimeProvider _time;

    public CreateTenantHandler(IDbContextFactory<AuthDbContext> factory, TimeProvider time)
    {
        _factory = factory;
        _time = time;
    }

    public IObservable<ErrorOr<Tenant>> Handle(Request message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<Tenant>> Handle(Request message, CancellationToken token)
    {
        await using var context = await _factory.CreateDbContextAsync(token);

        var user = await context.Users
            .AsNoTracking()
            .Where(x => x.Id == message.UserId)
            .FirstOrDefaultAsync(token);

        if (user is null)
        {
            return Error.NotFound();
        }

        var organisation = await context.Organisations
            .Include(x => x.Tenants)
            .Where(x => x.Slug == message.Organisation)
            .Where(x => x.CreatedBy == user.Id)
            .FirstOrDefaultAsync(token);

        if (organisation is null)
        {
            return Error.NotFound();
        }

        var slug = Slug.New(message.Name);

        var result = Tenant.New(message.Name, slug, user.Id, _time);

        if (result.IsError)
        {
            return result;
        }
        
        organisation.AddTenant(result.Value);

        await context.SaveChangesAsync(token);

        return result;
    }

    public sealed record Request(
        Guid UserId,
        string Organisation,
        string Name
    ) : IRequest<ErrorOr<Tenant>>;
}