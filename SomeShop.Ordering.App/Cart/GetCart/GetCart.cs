using CqrsVibe.Queries;
using CqrsVibe.Queries.Pipeline;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;
using SomeShop.Common.Exceptions;
using SomeShop.Ordering.EF;

namespace SomeShop.Ordering.App.Cart;

public class GetCartHandler : IQueryHandler<GetCart, GetCartModel>
{
    private readonly OrderingDbContext _dbContext;

    public GetCartHandler(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetCartModel> HandleAsync(IQueryHandlingContext<GetCart> context,
        CancellationToken cancellationToken = default)
    {
        const string getCartQuery =
            "SELECT id, total_products_positions, total_sum_amount, total_sum_currency " +
            "FROM ordering.carts " +
            "WHERE id=@id";

        const string getCartItemsQuery =
            "SELECT id, product_id, quantity, price_amount, price_currency " +
            "FROM ordering.cart_items " +
            "WHERE cart_id=@id";

        var queriesParams = new { id = context.Query.Id };

        var connection = _dbContext.Database.GetDbConnection();
        
        var cart = (await connection.QueryAsync<GetCartModel>(getCartQuery, queriesParams)).FirstOrDefault();
        if (cart == default)
        {
            throw new CartNotFoundException(context.Query.Id);
        }

        cart.Items =
            await connection.QueryAsync<GetCartModel.CartItemModel>(getCartItemsQuery, queriesParams) ??
            Enumerable.Empty<GetCartModel.CartItemModel>();

        return cart;
    }
}

public class GetCart : IQuery<GetCartModel>
{
    public GetCart(CartId id)
    {
        Id = id;
    }

    public CartId Id { get; }
}

public class GetCartModel
{
    public CartId Id { get; set; }
    
    public uint TotalProductsPositions { get; set; }
    
    public decimal TotalSumAmount { get; set; }
    
    public Currency TotalSumCurrency { get; set; }
    
    public IEnumerable<CartItemModel> Items { get; set; } = Enumerable.Empty<CartItemModel>();

    public class CartItemModel
    {
        public CartItemId Id { get; set; }
        
        public ProductId ProductId { get; set; }
        
        public decimal PriceAmount { get; set; }
        
        public Currency PriceCurrency { get; set; }
        
        public uint Quantity { get; set; }
    }
}

public class CartNotFoundException : NotFoundException
{
    public CartNotFoundException(CartId cartId) : base($"Cart with id = '{cartId.Value:D}' not found") {}
}