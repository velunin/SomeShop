using CqrsVibe.Commands;
using CqrsVibe.Commands.Pipeline;
using SomeShop.Common.Domain.Ids;
using SomeShop.Ordering.Domain;

namespace SomeShop.Ordering.App.Cart;

public class AddProductHandler : ICommandHandler<AddProduct>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICatalog _catalog;

    public AddProductHandler(ICartRepository cartRepository, ICatalog catalog)
    {
        _cartRepository = cartRepository;
        _catalog = catalog;
    }

    public async Task HandleAsync(ICommandHandlingContext<AddProduct> context, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.Get(context.Command.CartId, cancellationToken);
        
        await cart.Add(
            context.Command.ProductId, 
            context.Command.Quantity, 
            _catalog, 
            cancellationToken);

        await _cartRepository.Save(cart, cancellationToken);
    }
}
public class AddProduct : ICommand
{
    public AddProduct(CartId cartId, ProductId productId, Quantity quantity)
    {
        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
    }

    public CartId CartId { get; }
    
    public ProductId ProductId { get; }
    
    public Quantity Quantity { get; }
}