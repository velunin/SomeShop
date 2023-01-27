using CqrsVibe.AspNetCore;
using SomeShop.Api;
using SomeShop.Catalog.App;
using SomeShop.Catalog.EF;
using SomeShop.Common.App;
using SomeShop.Ordering.App;
using SomeShop.Ordering.EF;
using SomeShop.StockManagement.App;
using OrderingModule = SomeShop.Ordering.App.Module;
using CatalogModule = SomeShop.Catalog.App.Module;
using StockManagementModule = SomeShop.StockManagement.App.Module;

using CartV1 = SomeShop.Ordering.App.Api.Cart.V1;
using OrderV1 = SomeShop.Ordering.App.Api.Order.V1;
using CatalogV1 =  SomeShop.Catalog.App.Api.V1;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddConfigs(builder.Configuration)
    .AddKafka(configurator =>
    {
        StockManagementModule
            .ConfigureConsumers(configurator);
        OrderingModule
            .ConfigureConsumers(configurator);
    })
    .AddCommonServices()
    .AddCatalog()
    .AddOrdering()
    .AddStockManagement()
    .AddHostedService<KafkaConsumersBackgroundService>()
    .AddGrpc(options =>
    {
        options.Interceptors.Add<ExceptionInterceptor>();
    });

builder.Services.AddHealthChecks();

var app = builder.Build();

using var startupScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

await OrderingModule.Init(startupScope.ServiceProvider.GetRequiredService<OrderingDbContext>());
await CatalogModule.Init(startupScope.ServiceProvider.GetRequiredService<CatalogDbContext>());

app.MapGrpcService<CartV1.GrpcService>();
app.MapGrpcService<OrderV1.GrpcService>();
app.MapGrpcService<CatalogV1.GrpcService>();
app.MapHealthChecks("/ready");
app.UseCqrsVibe();

app.Run();