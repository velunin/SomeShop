using System.Text;
using Confluent.Kafka;
using SomeShop.Common.App.Configs;

namespace SomeShop.Common.App.Kafka;

public class Producer : IProducer
{
    private readonly IProducer<byte[], byte[]> _producer;

    public Producer(KafkaConfig config)
    {
        _producer = new ProducerBuilder<byte[], byte[]>(new ProducerConfig
        {
            BootstrapServers = config.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            Partitioner = Partitioner.Consistent
        }).Build();
    }

    public Task ProduceAsync(string topic, string key, byte[] message, CancellationToken cancellationToken)
    {
        return _producer.ProduceAsync(topic, new Message<byte[], byte[]>
        {
            Key = Encoding.UTF8.GetBytes(key),
            Value = message,
        }, cancellationToken);
    }
}