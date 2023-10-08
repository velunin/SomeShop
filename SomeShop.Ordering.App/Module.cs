using System.Data;
using CqrsVibe.MicrosoftDependencyInjection;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SomeShop.Common.App.Kafka;
using SomeShop.Common.Domain.Ids;
using SomeShop.Ordering.App.Cart;
using SomeShop.Ordering.App.Order;
using SomeShop.Ordering.App.Order.WhenOrderCreated.Outbox;
using SomeShop.Ordering.App.Order.WhenReceivedEvent;
using SomeShop.Ordering.Domain;
using SomeShop.Ordering.EF;
using SomeShop.StockManagement.Contracts;

namespace SomeShop.Ordering.App;

public static class Module
{
    private const string ConsumerGroup = "ordering-consumer-group";

    public static IServiceCollection AddOrdering(this IServiceCollection services)
    {
        services
            .AddCqrsVibe()
            .AddCqrsVibeHandlers(ServiceLifetime.Scoped, new[] { typeof(Module).Assembly })
            //Domain services
            .AddSingleton<ICatalog, Ordering.App.Cart.Catalog>()
            .AddScoped<ICartWithActualPrices,CartWithActualPrices>()
            //App services
            .AddScoped<IOrderCreatedOutbox, OrderCreatedOutbox>()
            //DB
            .AddScoped<ITransactionManager, OrderingTransactionManager>()
            .AddScoped<ICartRepository, CartRepository>()
            .AddScoped<IOrderRepository, OrderRepository>()
            .AddOrderingDb();

        return services;
    }

    public static void ConfigureConsumers(IRegistryConfigurator configurator)
    {
        configurator.Add<ProductsReservationResultConsumer>(ConsumerGroup, Topics.OrderProductsReservationResult);
    }

    public static async Task Init(OrderingDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        
        SqlMapper.AddTypeHandler(new CartIdTypeHandler());
        SqlMapper.AddTypeHandler(new CartItemIdTypeHandler());
        SqlMapper.AddTypeHandler(new OrderIdTypeHandler());
        SqlMapper.AddTypeHandler(new OrderItemIdTypeHandler());
        SqlMapper.AddTypeHandler(new ProductIdTypeHandler());
        SqlMapper.AddTypeHandler(new QuantityTypeHandler());
    }

    private class CartIdTypeHandler : SqlMapper.TypeHandler<CartId>
    {
        public override void SetValue(IDbDataParameter parameter, CartId value)
        {
            parameter.Value = value.Value;
        }

        public override CartId Parse(object value)
        {
            return new CartId((Guid)value);
        }
    }

    private class CartItemIdTypeHandler : SqlMapper.TypeHandler<CartItemId>
    {
        public override void SetValue(IDbDataParameter parameter, CartItemId value)
        {
            parameter.Value = value.Value;
        }

        public override CartItemId Parse(object value)
        {
            return new CartItemId((Guid)value);
        }
    }

    private class OrderIdTypeHandler : SqlMapper.TypeHandler<OrderId>
    {
        public override void SetValue(IDbDataParameter parameter, OrderId value)
        {
            parameter.Value = value.Value;
        }

        public override OrderId Parse(object value)
        {
            return new OrderId((Guid)value);
        }
    }

    private class OrderItemIdTypeHandler : SqlMapper.TypeHandler<OrderItemId>
    {
        public override void SetValue(IDbDataParameter parameter, OrderItemId value)
        {
            parameter.Value = value.Value;
        }

        public override OrderItemId Parse(object value)
        {
            return new OrderItemId((Guid)value);
        }
    }
    
    private class QuantityTypeHandler : SqlMapper.TypeHandler<Quantity>
    {
        public override void SetValue(IDbDataParameter parameter, Quantity value)
        {   
            parameter.Value = value.Value;
        }

        public override Quantity Parse(object value)
        {
            var v = Convert.ToUInt32((long)value);
            return new Quantity(v);
        }
    }
    
    private class ProductIdTypeHandler : SqlMapper.TypeHandler<ProductId>
    {
        public override void SetValue(IDbDataParameter parameter, ProductId value)
        {
            parameter.Value = value.Value; 
        }

        public override ProductId Parse(object value)
        {
            return new ProductId((Guid)value);
        }
    }
}
