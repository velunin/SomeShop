using System.Data;
using CqrsVibe.MicrosoftDependencyInjection;
using Dapper;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SomeShop.Common.App;
using SomeShop.Common.App.Kafka;
using SomeShop.Common.Domain.Ids;
using SomeShop.Ordering.App.Cart;
using SomeShop.Ordering.App.Order;
using SomeShop.Ordering.App.Order.WhenOrderCreated.Outbox;
using SomeShop.Ordering.App.Order.WhenReceivedEvent;
using SomeShop.Ordering.Domain;
using SomeShop.Ordering.EF;
using SomeShop.StockManagement.Contracts;

using CatalogGrpc = SomeShop.Catalog.V1;

namespace SomeShop.Ordering.App;

public static class Module
{
    private const string ConsumerGroup = "ordering-consumer-group";

    public static IServiceCollection AddOrdering(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddConfig<OrderingConfig>(configuration, OrderingConfig.Section)
            .AddCqrsVibe()
            .AddCqrsVibeHandlers(ServiceLifetime.Scoped, new[] { typeof(Module).Assembly })
            //Domain services
            .AddCatalogPort(configuration)
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

    /// The ICatalog port has two wirings and the rest of the module cannot tell them apart.
    /// Switch with Ordering:CatalogTransport (or the Ordering__CatalogTransport env variable).
    private static IServiceCollection AddCatalogPort(this IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration.GetSection(OrderingConfig.Section).Get<OrderingConfig>() ?? new OrderingConfig();

        if (config.CatalogTransport == CatalogTransport.InProcess)
        {
            return services.AddSingleton<ICatalog, InProcessCatalog>();
        }

        // A real deployment would use Grpc.Net.ClientFactory (AddGrpcClient) for channel lifetime,
        // retries and DNS refresh; kept manual here to keep the wiring visible in one place.
        return services
            .AddSingleton(_ => GrpcChannel.ForAddress(config.CatalogGrpcUrl))
            .AddSingleton(sp => new CatalogGrpc.Service.ServiceClient(sp.GetRequiredService<GrpcChannel>()))
            .AddSingleton<ICatalog, GrpcCatalog>();
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
