using Google.Protobuf;
using SomeShop.Common.App.Kafka;
using SomeShop.Common.Proto;
using SomeShop.Ordering.Contracts;
using SomeShop.Ordering.Order.V1;

namespace SomeShop.Ordering.App.Order.WhenOrderCreated.Outbox;

public class OrderCreatedOutbox : IOrderCreatedOutbox
{
    private readonly IProducer _producer;

    public OrderCreatedOutbox(IProducer producer)
    {
        _producer = producer;
    }

    public Task Push(OrderCreatedOutboxMessage message, CancellationToken cancellationToken)
    {
        // Out of scope by design: this is a fake outbox — the message goes straight to Kafka
        // from inside the caller's transaction. A real one writes to a table in that same
        // transaction and lets a relay publish it; the two methods below are the missing half.
        
        var orderCreatedV1 = new OrderCreatedMessage()
        {
            MessageId = message.MessageId.ToString(),
            OrderId = message.OrderId.ToString(),
        };

        foreach (var messageItem in message.Items)
        {
            orderCreatedV1.Items.Add(new OrderCreatedMessage.Types.OrderCreatedMessageItem
            {
                ProductId = messageItem.ProductId.ToString(),
                Quantity = messageItem.Quantity,
                Price = new Money(messageItem.PriceAmount, messageItem.PriceCurrency),
            });
        }
        
        return _producer.ProduceAsync(
            Topics.OrderCreatedTopic,
            message.OrderId.ToString(),
            orderCreatedV1.ToByteArray(),
            cancellationToken);
    }

    public Task<IEnumerable<OrderCreatedOutboxMessage>> GetItemsToSend(int batchSize,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SetAsSent(HashSet<Guid> idsOfSentMessages)
    {
        throw new NotImplementedException();
    }
}

public interface IOrderCreatedOutbox
{
    Task Push(OrderCreatedOutboxMessage message, CancellationToken cancellationToken);
    Task<IEnumerable<OrderCreatedOutboxMessage>> GetItemsToSend(int batchSize, CancellationToken cancellationToken);
    Task SetAsSent(HashSet<Guid> idsOfSentMessages);
}