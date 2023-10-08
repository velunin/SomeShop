using CqrsVibe.Events;
using SomeShop.Common.Domain;

namespace SomeShop.Common.App;

public class DomainEventsProcessor : IDomainEventsProcessor
{
    private readonly IEventDispatcher _eventDispatcher;

    public DomainEventsProcessor(IEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
    }

    public async Task Process(IAggregate aggregate, CancellationToken cancellationToken = default)
    {
        foreach (var @event in aggregate.Events)
        {
            await _eventDispatcher.DispatchAsync(@event, cancellationToken);
        }
    }
}

public interface IDomainEventsProcessor
{
    Task Process(IAggregate aggregate, CancellationToken cancellationToken = default);
}