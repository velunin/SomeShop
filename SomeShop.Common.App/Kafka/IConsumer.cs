using Confluent.Kafka;

namespace SomeShop.Common.App.Kafka;

public interface IConsumer
{
    Task HandleAsync(Message<byte[], byte[]> message, CancellationToken cancellationToken);
}
