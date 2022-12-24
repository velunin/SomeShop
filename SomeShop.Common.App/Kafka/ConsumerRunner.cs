using Confluent.Kafka;
using CqrsVibe.MicrosoftDependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SomeShop.Common.App.Configs;

namespace SomeShop.Common.App.Kafka;

public class ConsumerRunner : IConsumerRunner
{
    private readonly ILogger<ConsumerRunner> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumersRegistry _consumersRegistry;
    private readonly KafkaConfig _config;

    public ConsumerRunner(ILogger<ConsumerRunner> logger, IServiceScopeFactory scopeFactory,
        IConsumersRegistry consumersRegistry, KafkaConfig config)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _consumersRegistry = consumersRegistry;
        _config = config;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        var consumersByGroups = _consumersRegistry.Consumers
            .GroupBy(x => x.Group)
            .ToDictionary(x => x.Key, x => x.ToList());

        var consumersTasks = new List<Task>();
        foreach (var group in consumersByGroups)
        {
            var builder = new ConsumerBuilder<byte[], byte[]>(new ConsumerConfig
            {
                BootstrapServers = _config.BootstrapServers,
                EnableAutoCommit = false,
                GroupId = group.Key,
            });
            
            consumersTasks.AddRange(
                group.Value.Select(x=> BuildAndRunConsumer(builder, x, cancellationToken)));
        }

        await Task.WhenAll(consumersTasks);
    }
    
    private async Task BuildAndRunConsumer(ConsumerBuilder<byte[], byte[]> builder, RegistryEntry consumeRegistryEntry,
        CancellationToken cancellationToken)
    {
        using var consumer = builder.Build();
        
        try
        {
            consumer.Subscribe(consumeRegistryEntry.Topic);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = await consumer.ConsumeAsync(cancellationToken);
                    if (consumeResult.IsPartitionEOF || consumeResult.Message == null)
                    {
                        await Task.Yield();
                        continue;
                    }

                    using var scope = _scopeFactory.CreateScope();

                    scope.ServiceProvider.SetAsCurrentResolver();
                    var consumerInstance =
                        (IConsumer)scope.ServiceProvider.GetRequiredService(consumeRegistryEntry.ConsumerType);

                    try
                    {
                        await consumerInstance.HandleAsync(consumeResult.Message, cancellationToken);
                        consumer.Commit(consumeResult);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Consume error");

                        await Task.Yield();
                        await Task.Delay(1000, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Yield();
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }
}