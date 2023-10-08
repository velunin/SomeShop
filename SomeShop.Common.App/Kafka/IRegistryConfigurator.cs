namespace SomeShop.Common.App.Kafka;

public interface IRegistryConfigurator
{
    public IRegistryConfigurator Add<TConsumer>(string consumerGroup, string topic) where TConsumer : class, IConsumer;
}