using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SomeShop.Common.App.Configs;

namespace SomeShop.StockManagement.EF;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddStockManagementDb(this IServiceCollection services)
    {
        services.AddDbContextPool<StockManagementDbContext>((provider, builder) =>
        {
            var config = provider.GetRequiredService<PostgresConfig>();

            builder.UseNpgsql(config.ToConnectionString());
            builder.UseSnakeCaseNamingConvention();
        });

        return services;
    }
}
