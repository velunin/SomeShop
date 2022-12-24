namespace SomeShop.Common.App.Kafka;

public class RegistryEntry
{
    public RegistryEntry(Type consumerType, string group, string topic)
    {
        ConsumerType = consumerType;
        Group = group;
        Topic = topic;
    }

    public Type ConsumerType { get; }
    public string Group { get; }
    public string Topic { get; }
}