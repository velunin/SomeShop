using Confluent.Kafka;
using CqrsVibe.Commands;
using Microsoft.Extensions.Logging;
using SomeShop.Common.App.Kafka;
using SomeShop.Common.Domain.Ids;
using SomeShop.Ordering.Order.V1;
using SomeShop.StockManagement.App.ProductsReservation;
using SomeShop.StockManagement.Domain;

namespace SomeShop.StockManagement.App.ProductsReservation.WhenEventReceived;

public class OrderCreatedConsumer : IConsumer
{
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly ICommandProcessor _commandProcessor;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger, ICommandProcessor commandProcessor)
    {
        _logger = logger;
        _commandProcessor = commandProcessor;
    }

    public async Task HandleAsync(Message<byte[], byte[]> message, CancellationToken cancellationToken)
    {
        var orderCreated = Parse(message.Value);
        if (orderCreated == null)
        {
            return;
        }

        var items = orderCreated.Items
            .Select(x => new RequestedItem(new ProductId(Guid.Parse(x.ProductId)), x.Quantity))
            .ToArray();

        await _commandProcessor.ProcessAsync(
            new ReserveProducts(new OrderId(Guid.Parse(orderCreated.OrderId)), items),
            cancellationToken);
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
