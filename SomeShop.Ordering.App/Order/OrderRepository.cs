using Microsoft.EntityFrameworkCore;
using SomeShop.Common.App;
using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;
using SomeShop.Ordering.EF;

namespace SomeShop.Ordering.App.Order;

public class OrderRepository : IOrderRepository
{
    private readonly OrderingDbContext _dbContext;
    private readonly IDomainEventsProcessor _domainEventsProcessor;
    private readonly ITransactionManager _txManager;

    public OrderRepository(OrderingDbContext dbContext, IDomainEventsProcessor domainEventsProcessor,
        ITransactionManager txManager)
    {
        _dbContext = dbContext;
        _domainEventsProcessor = domainEventsProcessor;
        _txManager = txManager;
    }

    public async Task<Domain.Order> Get(OrderId id, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext
            .Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return order ?? throw new OrderNotFoundException(id);
    }

    public Task Add(Domain.Order aggregate, CancellationToken cancellationToken = default)
    {
        _dbContext.Add(aggregate);
        return SaveAndProcessEvents(aggregate, cancellationToken);
    }

    public async Task Save(Domain.Order aggregate, CancellationToken cancellationToken = default)
    {
        var orderFromDb = await _dbContext
                             .Orders
                             .Include(x => x.Items)
                             .FirstOrDefaultAsync(x => x.Id == aggregate.Id, cancellationToken) ??
                         throw new OrderNotFoundException(aggregate.Id);

        _dbContext.Entry(orderFromDb).CurrentValues.SetValues(aggregate);

        foreach (var orderItem in orderFromDb.Items)
        {
            if (aggregate.Items.All(x => x.Id != orderItem.Id))
            {
                _dbContext.OrderItems.Remove(orderItem);
            }
        }

        foreach (var orderItemNewState in aggregate.Items)
        {
            var cartItemFromDb = orderFromDb.Items.SingleOrDefault(x => x.Id == orderItemNewState.Id);

            if (cartItemFromDb != null)
            {
                _dbContext.Entry(cartItemFromDb).CurrentValues.SetValues(orderItemNewState);
            }
            else
            {
                _dbContext.OrderItems.Add(orderItemNewState);
            }
        }

        await SaveAndProcessEvents(aggregate, cancellationToken);
    }

    public async Task Delete(OrderId id, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ??
                    throw new OrderNotFoundException(id);
        
        _dbContext.Entry(model).State = EntityState.Deleted;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    private async Task SaveAndProcessEvents(IAggregate aggregate, CancellationToken cancellationToken)
    {
        if (_txManager.IsTransactionBegun())
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _domainEventsProcessor.Process(aggregate, cancellationToken);
            
            return;
        }

        await _txManager.ExecuteInTransaction(async () =>
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _domainEventsProcessor.Process(aggregate, cancellationToken);
        }, cancellationToken);
    }
}

public interface IOrderRepository : IRepository<Domain.Order, OrderId>
{
}