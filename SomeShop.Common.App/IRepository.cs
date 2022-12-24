using SomeShop.Common.Domain;

namespace SomeShop.Common.App;

public interface IRepository<TAggregate, in TIdentity> 
    where TAggregate : AggregateBase 
    where TIdentity : struct
{
    public Task<TAggregate> Get(TIdentity id, CancellationToken cancellationToken = default);
    public Task Add(TAggregate aggregate, CancellationToken cancellationToken = default);
    public Task Save(TAggregate aggregate, CancellationToken cancellationToken = default);
    public Task Delete(TIdentity id, CancellationToken cancellationToken = default);
}