using CqrsVibe.Commands;
using CqrsVibe.Commands.Pipeline;
using SomeShop.Common.Domain.Ids;

namespace SomeShop.Ordering.App.Cart.RemoveProduct;

public class RemoveProductHandler : ICommandHandler<RemoveProduct>
{
    private readonly ICartRepository _cartRepository;

    public RemoveProductHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task HandleAsync(ICommandHandlingContext<RemoveProduct> context, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.Get(context.Command.CartId, cancellationToken);

        cart.Remove(context.Command.ProductId);

        await _cartRepository.Save(cart, cancellationToken);
    }
}

public class RemoveProduct : ICommand
{
    public RemoveProduct(CartId cartId, ProductId productId)
    {
        CartId = cartId;
        ProductId = productId;
    }

    public CartId CartId { get; }
    
    public ProductId ProductId { get; }
}