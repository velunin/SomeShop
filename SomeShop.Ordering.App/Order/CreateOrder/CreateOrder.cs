using CqrsVibe.Commands;
using CqrsVibe.Commands.Pipeline;
using SomeShop.Common.Domain.Ids;
using SomeShop.Ordering.Domain;

namespace SomeShop.Ordering.App.Order;

public class CreateOrderHandler : ICommandHandler<CreateOrder, OrderId>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartWithActualPrices _cartWithActualPrices;

    public CreateOrderHandler(IOrderRepository orderRepository, ICartWithActualPrices cartWithActualPrices)
    {
        _orderRepository = orderRepository;
        _cartWithActualPrices = cartWithActualPrices;
    }

    public async Task<OrderId> HandleAsync(ICommandHandlingContext<CreateOrder> context, CancellationToken cancellationToken)
    {
        var order = await Domain.Order.Create(context.Command.CartId, _cartWithActualPrices, cancellationToken);
        
        await _orderRepository.Add(order, cancellationToken);
        
        return order.Id;
    }
}

public class CreateOrder : ICommand<OrderId>
{
    public CreateOrder(CartId cartId)
    {
        CartId = cartId;
    }

    public CartId CartId { get; }
}
