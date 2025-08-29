using System.Reactive.Linq;
using Depot.Auth.Domain.Auth;
using Depot.Auth.Domain.Tenants;
using Depot.Auth.Persistence;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Depot.Auth.Features.Organisations.CreateTenant;

public class CreateTenantHandler : IMessageHandler<CreateTenantHandler.Request, ErrorOr<Tenant>>
{
    private readonly AuthDbContext _context;

    private readonly TimeProvider _time;

    public CreateTenantHandler(AuthDbContext context, TimeProvider time)
    {
        _context = context;
        _time = time;
    }

    public IObservable<ErrorOr<Tenant>> Handle(Request message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<Tenant>> Handle(Request message, CancellationToken token)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == new UserId(message.UserId))
            .FirstOrDefaultAsync(token);

        if (user is null)
        {
            return Error.NotFound();
        }

        var organisation = await _context.Organisations
            .Include(x => x.Tenants)
            .Where(x => x.Slug == message.Organisation)
            .Where(x => x.CreatedBy == user.Id)
            .FirstOrDefaultAsync(token);

        if (organisation is null)
        {
            return Error.NotFound();
        }

        var result = organisation.AddTenant(message.Name, user.Id, _time);

        if (result.IsError)
        {
            return result;
        }

        await _context.SaveChangesAsync(token);

        return result;
    }

    public sealed record Request(
        Guid UserId,
        string Organisation,
        string Name
    ) : IRequest<ErrorOr<Tenant>>;
}