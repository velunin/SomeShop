using CqrsVibe.Events;
using CqrsVibe.Events.Pipeline;
using SomeShop.Ordering.App.Cart;
using SomeShop.Ordering.Domain;

namespace SomeShop.Ordering.App.Order.WhenCheckedOut;

public class ClearCart : IEventHandler<OrderCheckedOut>
{
    private readonly ICartRepository _cartRepository;

    public ClearCart(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task HandleAsync(IEventHandlingContext<OrderCheckedOut> context, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.Get(context.Event.CartId, cancellationToken);
        
        cart.Clear();

        await _cartRepository.Save(cart, cancellationToken);
    }
}
