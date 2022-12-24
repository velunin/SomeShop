using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SomeShop.Common.App.Configs;

namespace SomeShop.Ordering.EF;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddOrderingDb(this IServiceCollection services)
    {
        services.AddDbContextPool<OrderingDbContext>((provider, builder) =>
        {
            var config = provider.GetRequiredService<PostgresConfig>();

            builder.UseNpgsql(config.ToConnectionString());
            builder.UseSnakeCaseNamingConvention();
        });
        
        return services;
    }
}