namespace SomeShop.Common.Domain.Ids;

public readonly struct OrderId : IEquatable<OrderId>
{
    public static OrderId Create()
    {
        return new OrderId(Guid.NewGuid());
    }
    
    public OrderId(Guid value)
    {
        Value = value;
    }

    public OrderId(Guid? value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        Value = value.Value;
    }

    public Guid Value { get; }

    public bool Equals(OrderId other)
    {
        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        return obj is OrderId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(OrderId a, OrderId b)
    {
        return a.Equals(b);
    }
    
    public static bool operator !=(OrderId a, OrderId b)
    {
        return !(a == b);
    }
}