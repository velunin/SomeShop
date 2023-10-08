namespace SomeShop.Common.App.Kafka;

public interface IConsumerRunner
{
    Task Run(CancellationToken cancellationToken);
}