using Microsoft.Extensions.DependencyInjection;
using SomeShop.Common.App.Kafka;
using SomeShop.Ordering.Contracts;
using SomeShop.StockManagement.App.ProductsReservation.WhenEventReceived;

namespace SomeShop.StockManagement.App;

public static class Module
{
    private const string ConsumerGroup = "stock-management-consumer-group";

    public static IServiceCollection AddStockManagement(this IServiceCollection services)
    {
        return services;
    }

    public static void ConfigureConsumers(IRegistryConfigurator configurator)
    {
        configurator.Add<OrderCreatedConsumer>(ConsumerGroup, Topics.OrderCreatedTopic);
    }
}