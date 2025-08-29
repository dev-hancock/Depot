using Mestra.Abstractions;

namespace Depot.Auth.Domain.Common;

public abstract class Entity { }

public abstract class Root
{
    private readonly List<INotification> _events = [];

    public IEnumerable<INotification> Events => _events;

    public void Clear()
    {
        _events.Clear();
    }

    protected void Raise(INotification @event)
    {
        _events.Add(@event);
    }
}