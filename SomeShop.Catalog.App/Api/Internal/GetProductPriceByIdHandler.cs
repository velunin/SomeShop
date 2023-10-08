using CqrsVibe.Queries;
using CqrsVibe.Queries.Pipeline;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SomeShop.Catalog.Contracts.InternalApi;
using SomeShop.Catalog.EF;

namespace SomeShop.Catalog.App.Api.Internal;

public class GetProductPriceByIdHandler : IQueryHandler<GetProductPriceById, GetProductPriceModel>
{
    private readonly CatalogDbContext _dbContext;

    public GetProductPriceByIdHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProductPriceModel> HandleAsync(IQueryHandlingContext<GetProductPriceById> context,
        CancellationToken cancellationToken = default)
    {
        const string query = "SELECT id, price_amount, price_currency " +
                             "FROM catalog.products " +
                             "WHERE id=@id";

        var connection = _dbContext.Database.GetDbConnection();

        var product = (await connection.QueryAsync<GetProductPriceModel>(query, new { context.Query.Id }))
            .FirstOrDefault();
        if (product == default)
        {
            throw new ProductNotFoundException(context.Query.Id);
        }

        return product;
    }
}