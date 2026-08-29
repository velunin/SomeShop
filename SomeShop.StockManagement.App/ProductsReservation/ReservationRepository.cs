using Microsoft.EntityFrameworkCore;
using SomeShop.Common.App;
using SomeShop.Common.Domain;
using SomeShop.Common.Domain.Ids;
using SomeShop.Common.Exceptions;
using SomeShop.StockManagement.Domain;
using SomeShop.StockManagement.EF;

namespace SomeShop.StockManagement.App.ProductsReservation;

public class ReservationRepository : IReservationRepository
{
    private readonly StockManagementDbContext _dbContext;
    private readonly IDomainEventsProcessor _domainEventsProcessor;
    private readonly ITransactionManager _txManager;

    public ReservationRepository(
        StockManagementDbContext dbContext,
        IDomainEventsProcessor domainEventsProcessor,
        ITransactionManager txManager)
    {
        _dbContext = dbContext;
        _domainEventsProcessor = domainEventsProcessor;
        _txManager = txManager;
    }

    public async Task<Domain.Reservation> Get(OrderId id, CancellationToken cancellationToken = default)
    {
        var reservation = await _dbContext
            .Reservations
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.OrderId == id, cancellationToken);

        return reservation ?? throw new ReservationNotFoundException(id);
    }

    public Task<bool> Exists(OrderId id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Reservations.AnyAsync(x => x.OrderId == id, cancellationToken);
    }

    public Task Add(Domain.Reservation aggregate, CancellationToken cancellationToken = default)
    {
        _dbContext.Add(aggregate);
        return SaveAndProcessEvents(aggregate, cancellationToken);
    }

    public Task Save(Domain.Reservation aggregate, CancellationToken cancellationToken = default)
    {
        return SaveAndProcessEvents(aggregate, cancellationToken);
    }

    public async Task Delete(OrderId id, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Reservations.FirstOrDefaultAsync(x => x.OrderId == id, cancellationToken) ??
                    throw new ReservationNotFoundException(id);

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

public interface IReservationRepository : IRepository<Domain.Reservation, OrderId>
{
    Task<bool> Exists(OrderId id, CancellationToken cancellationToken = default);
}

public class ReservationNotFoundException : NotFoundException
{
    public ReservationNotFoundException(OrderId id) : base($"Reservation for order '{id.Value:D}' not found")
    {
    }
}
