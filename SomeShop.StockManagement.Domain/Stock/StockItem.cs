using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace SomeShop.StockManagement.Domain;

/// How much of a product is on the shelf and how much of it is already promised to orders.
/// The invariant this context exists to defend: never promise more than is available.
public class StockItem : AggregateBase
{
    public static StockItem Create(ProductId productId, uint available)
    {
        return new StockItem { ProductId = productId, Available = available };
    }

    public bool CanReserve(uint quantity)
    {
        return quantity > 0 && quantity <= Available;
    }

    public void Reserve(uint quantity)
    {
        if (!CanReserve(quantity))
        {
            throw new NotEnoughStockException(ProductId, quantity, Available);
        }

        Available -= quantity;
        Reserved += quantity;
    }

    public void Release(uint quantity)
    {
        if (quantity > Reserved)
        {
            throw new NothingToReleaseException(ProductId, quantity, Reserved);
        }

        Reserved -= quantity;
        Available += quantity;
    }

    private StockItem() { }

    public ProductId ProductId { get; private set; }
    public uint Available { get; private set; }
    public uint Reserved { get; private set; }

    // Mapped to the Postgres xmin system column: two orders racing for the last item make one of
    // them fail on the concurrency token instead of overselling.
    public uint RowVersion { get; private set; }
}

public class NotEnoughStockException : DomainException
{
    public NotEnoughStockException(ProductId productId, uint requested, uint available) : base(
        $"Unable to reserve {requested} of product '{productId.Value:D}': only {available} available")
    {
    }
}

public class NothingToReleaseException : DomainException
{
    public NothingToReleaseException(ProductId productId, uint requested, uint reserved) : base(
        $"Unable to release {requested} of product '{productId.Value:D}': only {reserved} reserved")
    {
    }
}
