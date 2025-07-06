namespace Depot.Auth.Domain.Common;

public interface IEvent
{
}

public abstract class AggregateRoot
{
    // TODO: Placeholder
    public List<IEvent> Events { get; set; } = [];
}