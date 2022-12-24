using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SomeShop.Common.App.Configs;

namespace SomeShop.Catalog.EF;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCatalogDb(this IServiceCollection services)
    {
        services.AddDbContextPool<CatalogDbContext>((provider, builder) =>
        {
            var config = provider.GetRequiredService<PostgresConfig>();

            builder.UseNpgsql(config.ToConnectionString());
            builder.UseSnakeCaseNamingConvention();
        });
        
        return services;
    }
}