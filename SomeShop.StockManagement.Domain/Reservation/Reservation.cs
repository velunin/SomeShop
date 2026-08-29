using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace SomeShop.StockManagement.Domain;

/// What this context promised to a single order. Keyed by the order it belongs to, which is what
/// makes a redelivered OrderCreated harmless: the reservation already exists.
public class Reservation : AggregateBase
{
    public static async Task<Reservation> Create(
        OrderId orderId,
        IReadOnlyCollection<RequestedItem> requestedItems,
        IStock stock,
        CancellationToken cancellationToken = default)
    {
        if (requestedItems.Count == 0)
        {
            throw new EmptyReservationException(orderId);
        }

        var reservation = new Reservation { OrderId = orderId };

        var stockItems = await stock.GetItems(
            requestedItems.Select(x => x.ProductId).ToArray(),
            cancellationToken);

        // A reservation is all or nothing, so the whole batch is checked before any stock moves.
        foreach (var requested in requestedItems)
        {
            if (!stockItems.TryGetValue(requested.ProductId, out var stockItem))
            {
                return reservation.Reject(ReservationFailReason.UnknownProduct);
            }

            if (!stockItem.CanReserve(requested.Quantity))
            {
                return reservation.Reject(ReservationFailReason.NotEnoughStock);
            }
        }

        foreach (var requested in requestedItems)
        {
            stockItems[requested.ProductId].Reserve(requested.Quantity);
            reservation._items.Add(new ReservationItem(orderId, requested.ProductId, requested.Quantity));
        }

        return reservation.Confirm();
    }

    private Reservation Confirm()
    {
        Status = ReservationStatus.Confirmed;
        ApplyEvent(new ReservationCompleted(OrderId, true));

        return this;
    }

    private Reservation Reject(ReservationFailReason reason)
    {
        Status = ReservationStatus.Rejected;
        FailReason = reason;

        // Only the outcome leaves this context — the reason stays here, it is nobody else's model.
        ApplyEvent(new ReservationCompleted(OrderId, false));

        return this;
    }

    private Reservation() { }

    public OrderId OrderId { get; private set; }
    public ReservationStatus Status { get; private set; }
    public ReservationFailReason FailReason { get; private set; }
    public IReadOnlyCollection<ReservationItem> Items => _items.AsReadOnly();

    public DateTimeOffset CreatedAt { get; private set; }
    public uint RowVersion { get; private set; }

    private readonly List<ReservationItem> _items = new();
}

public class ReservationItem
{
    internal ReservationItem(OrderId orderId, ProductId productId, uint quantity)
    {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
    }

    // ReSharper disable once UnusedMember.Local
    private ReservationItem() { }

    public OrderId OrderId { get; private set; }
    public ProductId ProductId { get; private set; }
    public uint Quantity { get; private set; }
}

public readonly struct RequestedItem
{
    public RequestedItem(ProductId productId, uint quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }

    public ProductId ProductId { get; }
    public uint Quantity { get; }
}

public enum ReservationStatus
{
    Confirmed,
    Rejected
}

public enum ReservationFailReason
{
    None,
    UnknownProduct,
    NotEnoughStock
}

public class ReservationCompleted : IEvent
{
    public ReservationCompleted(OrderId orderId, bool success)
    {
        OrderId = orderId;
        Success = success;
    }

    public OrderId OrderId { get; }
    public bool Success { get; }
}

public class EmptyReservationException : DomainException
{
    public EmptyReservationException(OrderId orderId) : base(
        $"Order '{orderId.Value:D}' has nothing to reserve")
    {
    }
}
