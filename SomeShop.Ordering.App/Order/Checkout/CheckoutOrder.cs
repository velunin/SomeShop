using CqrsVibe.Commands;
using CqrsVibe.Commands.Pipeline;
using SomeShop.Common.Domain.Ids;

namespace SomeShop.Ordering.App.Order;

public class CheckoutOrderHandler : ICommandHandler<CheckoutOrder>
{
    private readonly IOrderRepository _orderRepository;

    public CheckoutOrderHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task HandleAsync(ICommandHandlingContext<CheckoutOrder> context, CancellationToken cancellationToken = new CancellationToken())
    {
        var order = await _orderRepository.Get(context.Command.Id, cancellationToken);
        order.Checkout();
        await _orderRepository.Save(order, cancellationToken);
    }
}

public class CheckoutOrder : ICommand
{
    public CheckoutOrder(OrderId id)
    {
        Id = id;
    }

    public OrderId Id { get; }
}