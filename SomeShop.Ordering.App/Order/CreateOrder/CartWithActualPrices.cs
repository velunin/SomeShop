using CqrsVibe.Queries;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SomeShop.Catalog.Contracts.InternalApi;
using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;
using SomeShop.Ordering.Domain;
using SomeShop.Ordering.EF;

namespace SomeShop.Ordering.App.Order;

public class CartWithActualPrices : ICartWithActualPrices
{
    private readonly OrderingDbContext _dbContext;
    private readonly IQueryService _queryService;

    public CartWithActualPrices(OrderingDbContext dbContext, IQueryService queryService)
    {
        _dbContext = dbContext;
        _queryService = queryService;
    }

    public async Task<IList<CartItemWithActualPrice>> GetItems(CartId cartId, CancellationToken cancellationToken = default)
    {
        var productsFromCart = await GetProductsFromCart(cartId, cancellationToken);
        var productIds = productsFromCart.Select(x => x.ProductId).ToArray();

        var actualPrices = (await _queryService.QueryAsync(new GetProductsPricesByIds(productIds), cancellationToken))
            .ToDictionary(x => x.Id, x => x);
        
        var result = new List<CartItemWithActualPrice>(productsFromCart.Count);
        foreach (var item in productsFromCart)
        {
            if (!actualPrices.TryGetValue(item.ProductId, out var actualPrice))
            {
                throw new InvalidOperationException(
                    $"Failed to get actual price for product '{item.ProductId.Value:D}'");
            }

            result.Add(new CartItemWithActualPrice(
                item.ProductId, 
                item.Quantity,
                new Money(actualPrice.PriceAmount, actualPrice.PriceCurrency)));
        }

        return result;
    }

    private async Task<List<ProductsFromCartProjection>> GetProductsFromCart(CartId cartId, CancellationToken cancellationToken)
    {
        const string query = "select product_id, quantity from ordering.cart_items where cart_id = @cartId";
        var conn = _dbContext.Database.GetDbConnection();

        return (await conn.QueryAsync<ProductsFromCartProjection>(query, new { cartId })).ToList();
    }

    private class ProductsFromCartProjection
    {
        public ProductId ProductId { get; set; }
        
        public Quantity Quantity { get; set; }
    }
}