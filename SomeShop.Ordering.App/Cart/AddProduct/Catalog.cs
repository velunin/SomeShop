using CqrsVibe.Queries;
using SomeShop.Catalog.Contracts.InternalApi;
using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;
using SomeShop.Ordering.Domain;

namespace SomeShop.Ordering.App.Cart;

public class Catalog : ICatalog
{
    private readonly IQueryService _queryService;

    public Catalog(IQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<Product> GetProduct(ProductId id, CancellationToken cancellationToken)
    {
        var product = await _queryService.QueryAsync(new GetProductPriceById(id), cancellationToken);

        return new Product(product.Id, new Money(product.PriceAmount, product.PriceCurrency));
    }
}