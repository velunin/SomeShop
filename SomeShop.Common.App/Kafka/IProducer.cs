namespace SomeShop.Common.App.Kafka;

public interface IProducer
{
    Task ProduceAsync(string topic, string key, byte[] message, CancellationToken cancellationToken);
}