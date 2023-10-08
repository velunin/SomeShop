namespace SomeShop.Common.Domain.Ids;

public readonly struct CartItemId : IEquatable<CartItemId>
{
    public static CartItemId Create()
    {
        return new CartItemId(Guid.NewGuid());
    }
    
    public CartItemId(Guid value)
    {
        Value = value;
    }
    
    public CartItemId(Guid? value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }
        
        Value = value.Value;
    }

    public Guid Value { get; }

    public static bool operator ==(CartItemId a, CartItemId b)
    {
        return a.Value == b.Value;
    }
    
    public static bool operator !=(CartItemId a, CartItemId b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public bool Equals(CartItemId other)
    {
        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        return obj is CartItemId other && Equals(other);
    }
}