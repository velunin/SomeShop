using Confluent.Kafka;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SomeShop.Common.App.Kafka;
using SomeShop.Ordering.Order.V1;
using SomeShop.StockManagement.Contracts;
using SomeShop.StockManagement.Reservation.V1;

namespace SomeShop.StockManagement.App.ProductsReservation.WhenEventReceived;

public class OrderCreatedConsumer : IConsumer
{
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IProducer _producer;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger, IProducer producer)
    {
        _logger = logger;
        _producer = producer;
    }

    public async Task HandleAsync(Message<byte[], byte[]> message, CancellationToken cancellationToken)
    {
        var orderCreated = Parse(message.Value);
        if (orderCreated == null)
        {
            return;
        }

        var productsReservationResult = new OrderProductsReservationResultMessage
        {
            MessageId = Guid.NewGuid().ToString("D"),
            OrderId = orderCreated.OrderId,
            Success = true
        };

        await _producer.ProduceAsync(Topics.OrderProductsReservationResult, orderCreated.OrderId,
            productsReservationResult.ToByteArray(), cancellationToken);
    }

    private OrderCreatedMessage? Parse(byte[] bytes)
    {
        try
        {
            return OrderCreatedMessage.Parser.WithDiscardUnknownFields(true)
                .ParseFrom(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse OrderCreatedMessage. Skip");
            return null;
        }
    }
}