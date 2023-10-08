using CqrsVibe.Queries;
using CqrsVibe.Queries.Pipeline;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SomeShop.Catalog.EF;
using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;

namespace SomeShop.Catalog.App;

public class GetProductsHandler : IQueryHandler<GetProducts, GetProductsModel>
{
    private readonly CatalogDbContext _dbContext;

    public GetProductsHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProductsModel> HandleAsync(IQueryHandlingContext<GetProducts> context,
        CancellationToken cancellationToken = default)
    {
        var products = await _dbContext.Database.GetDbConnection()
            .QueryAsync<GetProductsModel.Product>(
                "select id, name, price_amount, price_currency from catalog.products",
                cancellationToken);

        return new GetProductsModel
        {
            Products = products ?? Enumerable.Empty<GetProductsModel.Product>()
        };
    }
}

public class GetProducts : IQuery<GetProductsModel>
{
    public GetProducts(Currency currency)
    {
        Currency = currency;
    }

    public Currency Currency { get; }
}

public class GetProductsModel
{
    public IEnumerable<Product> Products { get; set; } = Enumerable.Empty<Product>();
    
    public class Product
    {
        public ProductId Id { get; set; }

        public string Name { get; set; } = "";

        public decimal PriceAmount { get; set; }
        
        public Currency PriceCurrency { get; set; }
    }
}