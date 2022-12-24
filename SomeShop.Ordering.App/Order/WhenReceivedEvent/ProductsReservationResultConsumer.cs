using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using SomeShop.Common.App.Kafka;
using SomeShop.Common.Domain.Ids;
using SomeShop.StockManagement.Reservation.V1;

namespace SomeShop.Ordering.App.Order.WhenReceivedEvent;

// ReSharper disable once ClassNeverInstantiated.Global
public class ProductsReservationResultConsumer : IConsumer
{
    private readonly ILogger<ProductsReservationResultConsumer> _logger;
    private readonly IOrderRepository _orderRepository;

    public ProductsReservationResultConsumer(ILogger<ProductsReservationResultConsumer> logger, IOrderRepository orderRepository)
    {
        _logger = logger;
        _orderRepository = orderRepository;
    }

    public async Task HandleAsync(Message<byte[], byte[]> message, CancellationToken cancellationToken)
    {
        var result = Parse(message.Value);
        if (result == null)
        {
            return;
        }

        var order = await _orderRepository.Get(new OrderId(Guid.Parse(result.OrderId)), cancellationToken);

        if (result.Success)
        {
            order.WhenReservationOnStockConfirmed();
        }
        else
        {
            order.WhenReservationOnStockFailed();
        }

        await _orderRepository.Save(order, cancellationToken);
    }
    
    private OrderProductsReservationResultMessage? Parse(byte[] bytes)
    {
        try
        {
            return OrderProductsReservationResultMessage.Parser.WithDiscardUnknownFields(true)
                .ParseFrom(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse OrderProductsReservationResultMessage. Skip");
            return null;
        }
    }
}