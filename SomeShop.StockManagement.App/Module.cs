using CqrsVibe.MicrosoftDependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SomeShop.Common.App;
using SomeShop.Common.App.Kafka;
using SomeShop.Ordering.Contracts;
using SomeShop.StockManagement.App.ProductsReservation;
using SomeShop.StockManagement.App.ProductsReservation.WhenEventReceived;
using SomeShop.StockManagement.Domain;
using SomeShop.StockManagement.EF;

namespace SomeShop.StockManagement.App;

public static class Module
{
    private const string ConsumerGroup = "stock-management-consumer-group";

    public static IServiceCollection AddStockManagement(this IServiceCollection services)
    {
        return services
            .AddCqrsVibe()
            .AddCqrsVibeHandlers(ServiceLifetime.Scoped, new[] { typeof(Module).Assembly })
            //Domain services
            .AddScoped<IStock, Stock>()
            //DB
            .AddScoped<ITransactionManager, StockManagementTransactionManager>()
            .AddScoped<IReservationRepository, ReservationRepository>()
            .AddStockManagementDb();
    }

    public static void ConfigureConsumers(IRegistryConfigurator configurator)
    {
        configurator.Add<OrderCreatedConsumer>(ConsumerGroup, Topics.OrderCreatedTopic);
    }

    public static async Task Init(StockManagementDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
    }
}
