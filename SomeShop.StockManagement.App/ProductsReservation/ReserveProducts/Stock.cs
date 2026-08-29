using Microsoft.EntityFrameworkCore;
using SomeShop.Common.Domain.Ids;
using SomeShop.StockManagement.Domain;
using SomeShop.StockManagement.EF;

namespace SomeShop.StockManagement.App.ProductsReservation;

/// The adapter behind the IStock domain port.
public class Stock : IStock
{
    private readonly StockManagementDbContext _dbContext;

    public Stock(StockManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<ProductId, StockItem>> GetItems(
        IReadOnlyCollection<ProductId> productIds,
        CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.StockItems
            .Where(x => productIds.Contains(x.ProductId))
            .ToListAsync(cancellationToken);

        return items.ToDictionary(x => x.ProductId, x => x);
    }
}
