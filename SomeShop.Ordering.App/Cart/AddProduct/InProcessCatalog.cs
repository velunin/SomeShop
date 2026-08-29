using CqrsVibe.Queries;
using SomeShop.Catalog.Contracts.InternalApi;
using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;
using SomeShop.Ordering.Domain;

namespace SomeShop.Ordering.App.Cart;

/// The in-process wiring of the <see cref="ICatalog"/> port: the Catalog contract is
/// dispatched as a CQRS query inside the same host. See GrpcCatalog for the other wiring.
public class InProcessCatalog : ICatalog
{
    private readonly IQueryService _queryService;

    public InProcessCatalog(IQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<Product> GetProduct(ProductId id, CancellationToken cancellationToken)
    {
        var product = await _queryService.QueryAsync(new GetProductPriceById(id), cancellationToken);

        return new Product(product.Id, new Money(product.PriceAmount, product.PriceCurrency));
    }
}