namespace SomeShop.Common.Domain;

public interface IAggregate
{
    IReadOnlyCollection<IEvent> Events { get; }
}