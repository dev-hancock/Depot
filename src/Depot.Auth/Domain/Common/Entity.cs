namespace Depot.Auth.Domain.Common;

using Mestra.Abstractions;

public abstract class Entity
{
}

public abstract class Root
{
    private readonly List<INotification> _events = [];

    public IEnumerable<INotification> Events => _events;

    protected void Raise(INotification @event)
    {
        _events.Add(@event);
    }

    public void Clear()
    {
        _events.Clear();
    }
}