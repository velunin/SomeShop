// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;

#pragma warning disable CS8618

namespace SomeShop.Ordering.Domain;

public class CartItem
{
    internal CartItem(CartId cartId, Quantity quantity, Product product)
    {
        Id = CartItemId.Create();
        ProductId = product.Id;
        CartId = cartId;
        Price = product.Price;
        Quantity = quantity;
    }

    // ReSharper disable once UnusedMember.Local
    private CartItem()
    {
    }

    public CartItemId Id { get; private set; }
    public CartId CartId { get; private set; }
    public ProductId ProductId { get; private set; }
    public Money Price { get; private set; }
    public Quantity Quantity { get; internal set; }
    public Money Sum => Price * Quantity.Value;
}