using CqrsVibe.Commands;
using CqrsVibe.Commands.Pipeline;
using SomeShop.Common.Domain.Ids;

namespace SomeShop.Ordering.App.Cart;

public class ClearHandler : ICommandHandler<Clear>
{
    private readonly ICartRepository _cartRepository;

    public ClearHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task HandleAsync(ICommandHandlingContext<Clear> context, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.Get(context.Command.CartId, cancellationToken);

        cart.Clear();

        await _cartRepository.Save(cart, cancellationToken);
    }
}
public class Clear : ICommand
{
    public Clear(CartId cartId)
    {
        CartId = cartId;
    }

    public CartId CartId { get; }
}