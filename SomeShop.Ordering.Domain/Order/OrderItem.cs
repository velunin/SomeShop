using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;

namespace SomeShop.Ordering.Domain;

public class OrderItem
{
    internal OrderItem(OrderId orderId, ProductId productId, Quantity quantity, Money price)
    {
        Id = OrderItemId.Create();
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
    }
    
    private OrderItem() {}

    public OrderItemId Id { get; private set; }
    public OrderId OrderId { get; private set; }
    public ProductId ProductId { get; private set; }
    public Quantity Quantity { get; private set; }
    public Money Price { get; private set; }
}