using SomeShop.Common.Domain.Ids;

namespace SomeShop.Ordering.Domain;

public interface ICatalog
{
    Task<Product> GetProduct(ProductId id, CancellationToken cancellationToken);
}
