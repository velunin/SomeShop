using SomeShop.Common.App.Kafka;

namespace SomeShop.Api;

public class KafkaConsumersBackgroundService : BackgroundService
{
    private readonly IConsumerRunner _consumerRunner;

    public KafkaConsumersBackgroundService(IConsumerRunner consumerRunner)
    {
        _consumerRunner = consumerRunner;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _consumerRunner.Run(stoppingToken);
    }
}