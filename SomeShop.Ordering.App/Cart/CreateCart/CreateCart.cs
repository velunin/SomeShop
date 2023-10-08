using CqrsVibe.Commands;
using CqrsVibe.Commands.Pipeline;
using SomeShop.Common.Domain.Ids;

namespace SomeShop.Ordering.App.Cart;

public class CreateCartHandler : ICommandHandler<CreateCart, CartId>
{
    private readonly ICartRepository _cartRepository;

    public CreateCartHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<CartId> HandleAsync(ICommandHandlingContext<CreateCart> context, CancellationToken cancellationToken = default)
    {
        var cart = Domain.Cart.Create();
        await _cartRepository.Add(cart, cancellationToken);
        return cart.Id;
    }
}

public class CreateCart : ICommand<CartId>
{
}
