using CqrsVibe.Commands;
using CqrsVibe.Commands.Pipeline;
using SomeShop.Common.Domain.Ids;
using SomeShop.Ordering.Domain;

namespace SomeShop.Ordering.App.Cart;

public class ChangeQuantityHandler : ICommandHandler<ChangeQuantity>
{
    private readonly ICartRepository _cartRepository;

    public ChangeQuantityHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task HandleAsync(ICommandHandlingContext<ChangeQuantity> context, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.Get(context.Command.CartId, cancellationToken);
        
        cart.ChangeQuantity(context.Command.ProductId, context.Command.Quantity);

        await _cartRepository.Save(cart, cancellationToken);
    }
}

public class ChangeQuantity : ICommand
{
    public ChangeQuantity(CartId cartId, ProductId productId, Quantity quantity)
    {
        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
    }

    public CartId CartId { get; }
    
    public ProductId ProductId { get; }
    
    public Quantity Quantity { get; }
}