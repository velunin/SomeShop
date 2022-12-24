using CqrsVibe.Queries;
using CqrsVibe.Queries.Pipeline;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SomeShop.Catalog.Contracts.InternalApi;
using SomeShop.Catalog.EF;

namespace SomeShop.Catalog.App.Api.Internal;

public class GetProductsPricesByIdsHandler : IQueryHandler<GetProductsPricesByIds, List<GetProductPriceModel>>
{
    private readonly CatalogDbContext _dbContext;

    public GetProductsPricesByIdsHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<GetProductPriceModel>> HandleAsync(IQueryHandlingContext<GetProductsPricesByIds> context,
        CancellationToken cancellationToken)
    {
        const string query = "SELECT id, price_amount, price_currency " +
                             "FROM catalog.products " +
                             "WHERE id = ANY(@productIds)";
        
        var connection = _dbContext.Database.GetDbConnection();

        var uuids = context.Query.ProductIds.Select(x => x.Value).ToArray();
        var productsPrices =
            await connection.QueryAsync<GetProductPriceModel>(query, new { productIds = uuids }) ??
            Enumerable.Empty<GetProductPriceModel>();

        return productsPrices.ToList();
    }
}