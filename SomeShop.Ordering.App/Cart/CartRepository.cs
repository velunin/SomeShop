using Microsoft.EntityFrameworkCore;
using SomeShop.Common.App;
using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;
using SomeShop.Ordering.EF;

namespace SomeShop.Ordering.App.Cart;

public class CartRepository : ICartRepository
{
    private readonly OrderingDbContext _dbContext;
    private readonly ITransactionManager _txManager;
    private readonly IDomainEventsProcessor _domainEventsProcessor;

    public CartRepository(
        OrderingDbContext dbContext, 
        ITransactionManager txManager, 
        IDomainEventsProcessor domainEventsProcessor)
    {
        _dbContext = dbContext;
        _txManager = txManager;
        _domainEventsProcessor = domainEventsProcessor;
    }

    public async Task<Domain.Cart> Get(CartId id, CancellationToken cancellationToken = default)
    {
        var cart = await _dbContext
            .Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return cart ?? throw new CartNotFoundException(id);
    }

    public async Task Add(Domain.Cart aggregate, CancellationToken cancellationToken = default)
    {
        _dbContext.Add(aggregate);
        await SaveAndProcessEvents(aggregate, cancellationToken);
    }

    public async Task Save(Domain.Cart aggregate, CancellationToken cancellationToken = default)
    {
        await SaveAndProcessEvents(aggregate, cancellationToken);
    }

    public async Task Delete(CartId id, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Carts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ??
                    throw new CartNotFoundException(id);
        
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

public interface ICartRepository : IRepository<Domain.Cart, CartId>
{
}