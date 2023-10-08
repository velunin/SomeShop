using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace SomeShop.Ordering.Domain;

public class Order : AggregateBase
{
    public static async Task<Order> Create(
        CartId cartId, 
        ICartWithActualPrices cartWithActualPrices,
        CancellationToken cancellationToken = default)
    {
        var cartItems = await cartWithActualPrices.GetItems(cartId, cancellationToken);
        if (!cartItems.Any())
        {
            throw new CartIsEmptyException();
        }

        var order = new Order
        {
            Id = OrderId.Create(),
            CartId = cartId,
            Status = OrderStatus.Initial,
        };
        
        var sum = 0m;
        foreach (var cartItem in cartItems)
        {
            order._items.Add(new OrderItem(
                order.Id,
                cartItem.ProductId, 
                cartItem.Quantity, 
                cartItem.ActualPrice));
            sum += cartItem.ActualPrice.Amount;
        }

        order.TotalSum = new Money(sum, order._items.First().Price.Currency);
        
        order.ApplyEvent(new OrderCreated(
            order.Id, 
            order.Items));

        return order;
    }

    public void WhenReservationOnStockConfirmed()
    {
        if (ReservationStatus != ReservationStatus.Awaiting)
        {
            throw new ChangeReservationStatusException(ReservationStatus);
        }

        ReservationStatus = ReservationStatus.WasReserved;
        Status = OrderStatus.Reserved;
    }
    
    public void WhenReservationOnStockFailed()
    {
        if (ReservationStatus != ReservationStatus.Awaiting)
        {
            throw new ChangeReservationStatusException(ReservationStatus);
        }

        ReservationStatus = ReservationStatus.WasFailed;
        
        Fail(OrderFailReason.ReservationFailed);
    }

    public void Checkout()
    {
        if (ReservationStatus != ReservationStatus.WasReserved)
        {
            throw new OrderShouldBeReservedException(Status);
        }

        Status = OrderStatus.CheckedOut;
        ApplyEvent(new OrderCheckedOut(Id, CartId));
    }

    private void Fail(OrderFailReason reason)
    {
        Status = OrderStatus.Failed;
        FailReason = reason;
        
        ApplyEvent(new OrderFailed(Id, reason));
    }

    private Order() { }
    
    public OrderId Id { get; private set; }
    public CartId CartId { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public OrderStatus Status { get; private set; }
    public OrderFailReason FailReason { get; private set; }
    public ReservationStatus ReservationStatus { get; private set; }
    public Money TotalSum { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public uint RowVersion { get; private set; }
    
    private readonly List<OrderItem> _items = new();
}

public enum ReservationStatus
{
    Awaiting,
    WasReserved,
    WasFailed
}

public enum OrderStatus
{
    Initial,
    Reserved,
    CheckedOut,
    
    Failed,
}

public enum OrderFailReason
{
    None,
    ReservationFailed,
}

public class OrderCreated : IEvent
{
    public OrderCreated(OrderId orderId, IEnumerable<OrderItem> items)
    {
        OrderId = orderId;
        Items = items.Select(x => new CreatedOrderItem(x)).ToList();
    }
    
    public OrderId OrderId { get; }
    public List<CreatedOrderItem> Items { get; }

    public class CreatedOrderItem
    {
        public CreatedOrderItem(OrderItem orderItem)
        {
            ProductId = orderItem.ProductId;
            Quantity = orderItem.Quantity;
            Price = orderItem.Price;
        }
        
        public ProductId ProductId { get; }
        public Quantity Quantity { get; }
        public Money Price { get; }
    }
}

public class OrderCheckedOut : IEvent
{
    public OrderCheckedOut(OrderId orderId, CartId cartId)
    {
        OrderId = orderId;
        CartId = cartId;
    }

    public OrderId OrderId { get; }
    public CartId CartId { get; }
}

public class OrderFailed : IEvent
{
    public OrderFailed(OrderId orderId, OrderFailReason reason)
    {
        OrderId = orderId;
        Reason = reason;
    }

    public OrderId OrderId { get; }
    
    public OrderFailReason Reason { get; }
}

public class CartIsEmptyException : DomainException
{
    public CartIsEmptyException() : base("Cart is empty")
    {
    }
}

public class OrderShouldBeReservedException : DomainException
{
    public OrderShouldBeReservedException(OrderStatus status) : base(
        $"Unable to checkout order due to the wrong status: {status:G}")
    {
    }
}

public class ChangeReservationStatusException : DomainException
{
    public ChangeReservationStatusException(ReservationStatus status) : base(
        $"Reservation status must be {ReservationStatus.Awaiting:G}. Actual status: {status:G}")
    {
    }
}