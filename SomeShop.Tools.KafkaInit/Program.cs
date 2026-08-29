using Confluent.Kafka;
using Confluent.Kafka.Admin;

using OrderingTopics = SomeShop.Ordering.Contracts.Topics;
using StockManagementTopics = SomeShop.StockManagement.Contracts.Topics;

// Creates the topics the modules subscribe to. The consumers fail hard on an
// unknown topic, so this has to complete before the API starts.
// Topic names come from the contracts, so they cannot drift from the code.

var bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "localhost:9091";
var timeout = TimeSpan.FromSeconds(30);

var topics = new[]
{
    OrderingTopics.OrderCreatedTopic,
    StockManagementTopics.OrderProductsReservationResult
};

using var adminClient = new AdminClientBuilder(new AdminClientConfig
{
    BootstrapServers = bootstrapServers
}).Build();

var deadline = DateTime.UtcNow + timeout;
while (true)
{
    try
    {
        await adminClient.CreateTopicsAsync(topics.Select(topic => new TopicSpecification
        {
            Name = topic,
            NumPartitions = 1,
            ReplicationFactor = 1
        }));

        break;
    }
    catch (CreateTopicsException ex) when (ex.Results.All(r =>
                                               r.Error.Code is ErrorCode.NoError or ErrorCode.TopicAlreadyExists))
    {
        break;
    }
    catch (Exception ex) when (DateTime.UtcNow < deadline)
    {
        Console.WriteLine($"Waiting for {bootstrapServers}: {ex.Message}");
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
}

foreach (var topic in topics)
{
    Console.WriteLine($"Topic ready: {topic}");
}
