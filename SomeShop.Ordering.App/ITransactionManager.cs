using SomeShop.Common.EF;
using SomeShop.Ordering.EF;

namespace SomeShop.Ordering.App;

public interface ITransactionManager : Common.App.ITransactionManager
{
}

public class OrderingTransactionManager : TransactionManager<OrderingDbContext>, ITransactionManager
{
    public OrderingTransactionManager(OrderingDbContext dbContext) : base(dbContext)
    {
    }
}