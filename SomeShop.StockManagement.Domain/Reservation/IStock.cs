using SomeShop.Common.Domain.Ids;

namespace SomeShop.StockManagement.Domain;

/// The port the reservation needs to see the shelf. Implemented in the application layer.
public interface IStock
{
    Task<IReadOnlyDictionary<ProductId, StockItem>> GetItems(
        IReadOnlyCollection<ProductId> productIds,
        CancellationToken cancellationToken = default);
}
