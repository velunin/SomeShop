using CqrsVibe.Events;
using CqrsVibe.Events.Pipeline;
using Google.Protobuf;
using SomeShop.Common.App.Kafka;
using SomeShop.StockManagement.Contracts;
using SomeShop.StockManagement.Domain;
using SomeShop.StockManagement.Reservation.V1;

namespace SomeShop.StockManagement.App.ProductsReservation.WhenReservationCompleted;

/// This context publishes a fact, not a command surface: consumers learn the outcome and cannot
/// tell it what to reserve. The fail reason stays inside the context on purpose.
public class PublishResult : IEventHandler<ReservationCompleted>
{
    private readonly IProducer _producer;

    public PublishResult(IProducer producer)
    {
        _producer = producer;
    }

    public Task HandleAsync(IEventHandlingContext<ReservationCompleted> context,
        CancellationToken cancellationToken)
    {
        // Out of scope by design: published straight from the transaction, the same fake outbox
        // as the Ordering side.
        var message = new OrderProductsReservationResultMessage
        {
            MessageId = Guid.NewGuid().ToString("D"),
            OrderId = context.Event.OrderId.Value.ToString("D"),
            Success = context.Event.Success
        };

        return _producer.ProduceAsync(
            Topics.OrderProductsReservationResult,
            message.OrderId,
            message.ToByteArray(),
            cancellationToken);
    }
}
