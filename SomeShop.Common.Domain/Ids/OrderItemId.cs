namespace SomeShop.Common.Domain.Ids;

public readonly struct OrderItemId : IEquatable<OrderItemId>
{
    public static OrderItemId Create()
    {
        return new OrderItemId(Guid.NewGuid());
    }
    
    public OrderItemId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public bool Equals(OrderItemId other)
    {
        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        return obj is OrderItemId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(OrderItemId a, OrderItemId b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(OrderItemId a, OrderItemId b)
    {
        return !(a == b);
    }
}