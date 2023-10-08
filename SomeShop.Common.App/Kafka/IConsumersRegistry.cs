namespace SomeShop.Common.App.Kafka;

public interface IConsumersRegistry
{
    IReadOnlyCollection<RegistryEntry> Consumers { get; }
}