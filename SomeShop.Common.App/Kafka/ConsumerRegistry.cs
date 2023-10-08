namespace SomeShop.Common.App.Kafka;

public class ConsumersRegistry : IConsumersRegistry, IRegistryConfigurator
{
    public IReadOnlyCollection<RegistryEntry> Consumers => _consumers.AsReadOnly();
    private readonly List<RegistryEntry> _consumers = new();
    
    public IRegistryConfigurator Add<TConsumer>(string consumerGroup, string topic) where TConsumer : class, IConsumer
    {
        
        if (string.IsNullOrWhiteSpace(consumerGroup))
        {
            throw new ArgumentException(nameof(consumerGroup));
        }
        
        if (string.IsNullOrWhiteSpace(topic))
        {
            throw new ArgumentException(nameof(topic));
        }
        
        _consumers.Add(new RegistryEntry(typeof(TConsumer), consumerGroup, topic));
        
        return this;
    }
    
}