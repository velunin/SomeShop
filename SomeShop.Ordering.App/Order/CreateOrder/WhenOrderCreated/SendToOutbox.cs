using CqrsVibe.Events;
using CqrsVibe.Events.Pipeline;
using SomeShop.Ordering.App.Order.WhenOrderCreated.Outbox;
using SomeShop.Ordering.Domain;

namespace SomeShop.Ordering.App.Order.WhenOrderCreated;

public class SendToOutbox : IEventHandler<OrderCreated>
{
    private readonly IOrderCreatedOutbox _outbox;

    public SendToOutbox(IOrderCreatedOutbox outbox)
    {
        _outbox = outbox;
    }

    public Task HandleAsync(IEventHandlingContext<OrderCreated> context, CancellationToken cancellationToken)
    {
        return _outbox.Push(new OrderCreatedOutboxMessage
        {
            MessageId = Guid.NewGuid(),
            OrderId = context.Event.OrderId.Value,
            Items = context.Event.Items.Select(x => new OrderCreatedOutboxMessage.Item
            {
                ProductId = x.ProductId.Value,
                Quantity = x.Quantity.Value,
                PriceAmount = x.Price.Amount,
                PriceCurrency = x.Price.Currency
            }).ToList()
        }, cancellationToken);
    }
}
