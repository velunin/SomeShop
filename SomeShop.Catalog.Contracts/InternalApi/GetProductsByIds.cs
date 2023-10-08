using CqrsVibe.Queries;
using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;

namespace SomeShop.Catalog.Contracts.InternalApi;

public class GetProductsPricesByIds : IQuery<List<GetProductPriceModel>>
{
    public GetProductsPricesByIds(ProductId[] productIds)
    {
        if (productIds.Length == 0)
        {
            throw new ArgumentException(nameof(productIds));
        }
        
        ProductIds = productIds;
    }

    public ProductId[] ProductIds { get; }
}