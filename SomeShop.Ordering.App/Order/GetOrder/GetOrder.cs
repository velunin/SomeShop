using CqrsVibe.Queries;
using CqrsVibe.Queries.Pipeline;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;
using SomeShop.Common.Exceptions;
using SomeShop.Ordering.EF;

namespace SomeShop.Ordering.App.Order;

public class GetOrderHandler : IQueryHandler<GetOrder, GetOrderModel>
{
    private readonly OrderingDbContext _dbContext;

    public GetOrderHandler(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetOrderModel> HandleAsync(IQueryHandlingContext<GetOrder> context, CancellationToken cancellationToken)
    {
        const string getOrderQuery =
            "SELECT id, total_sum_amount, total_sum_currency " +
            "FROM ordering.orders " +
            "WHERE id=@id";

        const string getOrderItemsQuery =
            "SELECT id, product_id, quantity, price_amount, price_currency " +
            "FROM ordering.order_items " +
            "WHERE order_id=@id";

        var queriesParams = new { id = context.Query.Id };

        var connection = _dbContext.Database.GetDbConnection();
        
        var order = (await connection.QueryAsync<GetOrderModel>(getOrderQuery, queriesParams)).FirstOrDefault();
        if (order == default)
        {
            throw new OrderNotFoundException(context.Query.Id);
        }

        order.Items =
            await connection.QueryAsync<GetOrderModel.OrderItemModel>(getOrderItemsQuery, queriesParams) ??
            Enumerable.Empty<GetOrderModel.OrderItemModel>();

        return order;
    }
}

public class GetOrder : IQuery<GetOrderModel>
{
    public GetOrder(OrderId id)
    {
        Id = id;
    }

    public OrderId Id { get; }
}

public class GetOrderModel
{
    public OrderId Id { get; set; }

    public decimal TotalSumAmount { get; set; }
    
    public Currency TotalSumCurrency { get; set; }
    
    public IEnumerable<OrderItemModel> Items { get; set; } = Enumerable.Empty<OrderItemModel>();

    public class OrderItemModel
    {
        public OrderItemId Id { get; set; }
        
        public ProductId ProductId { get; set; }
        
        public decimal PriceAmount { get; set; }
        
        public Currency PriceCurrency { get; set; }
        
        public uint Quantity { get; set; }
    }
}

public class OrderNotFoundException : NotFoundException
{
    public OrderNotFoundException(OrderId orderId) : base($"Order with id = '{orderId.Value:D}' not found") {}
}