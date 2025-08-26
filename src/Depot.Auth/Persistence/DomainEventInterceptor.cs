namespace Depot.Auth.Persistence;

using System.Reactive.Threading.Tasks;
using Domain.Common;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class DomainEventsInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData data,
        InterceptionResult<int> result,
        CancellationToken token = default)
    {
        var context = data.Context;

        if (context == null)
        {
            return result;
        }

        var entities = context.ChangeTracker
            .Entries<Root>()
            .Select(x => x.Entity)
            .ToList();

        foreach (var entity in entities)
        {
            foreach (var notification in entity.Events)
            {
                await mediator.Publish(notification).ToTask(token);
            }

            entity.Clear();
        }

        return result;
    }
}