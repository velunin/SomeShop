using SomeShop.Common.App;
using SomeShop.Common.EF;
using SomeShop.StockManagement.EF;

namespace SomeShop.StockManagement.App;

public class StockManagementTransactionManager : TransactionManager<StockManagementDbContext>, ITransactionManager
{
    public StockManagementTransactionManager(StockManagementDbContext dbContext) : base(dbContext)
    {
    }
}
