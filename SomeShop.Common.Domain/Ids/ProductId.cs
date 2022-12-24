namespace SomeShop.Common.Domain.Ids;

public readonly struct ProductId : IEquatable<ProductId>
{
    public static ProductId Create()
    {
        return new ProductId(Guid.NewGuid());
    }
    
    public ProductId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static bool operator ==(ProductId a, ProductId b)
    {
        return a.Value == b.Value;
    }

    public static bool operator !=(ProductId a, ProductId b)
    {
        return a.Value != b.Value;
    }
    
    public bool Equals(ProductId other)
    {
        return Value.Equals(other.Value);
    }
    
    public override bool Equals(object? obj)
    {
        return obj is ProductId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}