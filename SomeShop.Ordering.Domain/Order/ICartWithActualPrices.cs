using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;

namespace SomeShop.Ordering.Domain;

public interface ICartWithActualPrices
{
    Task<IList<CartItemWithActualPrice>> GetItems(CartId cartId,
        CancellationToken cancellationToken = default);
}

public struct CartItemWithActualPrice
{
    public CartItemWithActualPrice(ProductId productId, Quantity quantity, Money actualPrice)
    {
        ProductId = productId;
        Quantity = quantity;
        ActualPrice = actualPrice;
    }

    public ProductId ProductId { get; }
    public Quantity Quantity { get;}
    public Money ActualPrice { get; }
}
