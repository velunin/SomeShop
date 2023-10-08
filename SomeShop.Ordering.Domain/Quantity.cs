namespace SomeShop.Ordering.Domain;

public struct Quantity : IEquatable<Quantity>
{
    public Quantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidQuantityException();
        }

        Value = (uint)quantity;
    }
    
    public Quantity(uint quantity) : this((int)quantity)
    {
    }
    
    public uint Value { get; private set; }
    

    public static implicit operator Quantity(int q) => new(q);

    public bool Equals(Quantity other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Quantity other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (int)Value;
    }
}