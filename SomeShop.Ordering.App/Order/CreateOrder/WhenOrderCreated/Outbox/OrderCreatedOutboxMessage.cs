using SomeShop.Common.Domain;

namespace SomeShop.Ordering.App.Order.WhenOrderCreated.Outbox;

public class OrderCreatedOutboxMessage
{
    public Guid MessageId { get; init; }
    public Guid OrderId { get; init; }
    public List<Item> Items { get; init; } = new();

    public class Item
    {
        public Guid ProductId { get; init; }
        public uint Quantity { get; init; }
        public decimal PriceAmount { get; init; }
        public Currency PriceCurrency { get; init; }
    }
}