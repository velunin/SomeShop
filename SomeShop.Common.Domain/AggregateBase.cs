namespace SomeShop.Common.Domain;

public class AggregateBase : IAggregate
{
    private readonly List<IEvent> _events = new();

    protected void ApplyEvent(IEvent @event)
    {
        _events.Add(@event);
    }

    public IReadOnlyCollection<IEvent> Events => _events.AsReadOnly();
}