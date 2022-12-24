using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SomeShop.Ordering.Domain;

public struct Product
{
    public Product(ProductId id, Money price)
    {
        if (price.Amount < 0)
        {
            throw new InvalidProductPriceException();
        }
        
        Id = id;
        Price = price;
    }

    public ProductId Id { get; }
    public Money Price { get; }
}

public class InvalidProductPriceException : DomainException
{
    public InvalidProductPriceException() : base("Price can't be less than 0")
    {
    }
}