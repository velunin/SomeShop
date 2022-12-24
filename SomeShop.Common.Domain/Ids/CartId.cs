namespace SomeShop.Common.Domain.Ids;

public readonly struct CartId : IEquatable<CartId>
{
    public static CartId Create()
    {
        return new CartId(Guid.NewGuid());
    }
    
    public CartId(Guid value)
    {
        Value = value;
    }

    public CartId(Guid? value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }
        
        Value = value.Value;
    }

    public Guid Value { get; }

    public static bool operator ==(CartId a, CartId b)
    {
        return a.Value == b.Value;
    }
    
    public static bool operator !=(CartId a, CartId b)
    {
        return a.Value != b.Value;
    }
    
    public bool Equals(CartId other)
    {
        return Value.Equals(other.Value);
    }
    
    public override bool Equals(object? obj)
    {
        return obj is CartId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}