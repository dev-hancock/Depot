namespace Depot.Auth.Domain.Common;

using Mestra.Abstractions;

public interface IEvent : INotification
{
}

public abstract class AggregateRoot
{
    // TODO: Placeholder
    public List<IEvent> Events { get; set; } = [];
}