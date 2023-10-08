using System.Data;
using CqrsVibe.MicrosoftDependencyInjection;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SomeShop.Catalog.EF;
using SomeShop.Common.Domain.Ids;

namespace SomeShop.Catalog.App;

public static class Module
{
    public static IServiceCollection AddCatalog(this IServiceCollection services)
    {
        return services
            .AddCqrsVibe()
            .AddCqrsVibeHandlers(ServiceLifetime.Scoped, new[] { typeof(Module).Assembly })
            .AddCatalogDb();
    }
    
    public static async Task Init(CatalogDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new ProductIdTypeHandler());
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