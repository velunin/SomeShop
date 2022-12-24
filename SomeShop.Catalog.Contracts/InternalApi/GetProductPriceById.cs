using CqrsVibe.Queries;
using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;

namespace SomeShop.Catalog.Contracts.InternalApi;

public class GetProductPriceById : IQuery<GetProductPriceModel>
{
    public GetProductPriceById(ProductId id)
    {
        Id = id;
    }

    public ProductId Id { get; }
}

public class GetProductPriceModel
{
    public ProductId Id { get; set; }
    
    public decimal PriceAmount { get; set; }
    
    public Currency PriceCurrency { get; set; }
}