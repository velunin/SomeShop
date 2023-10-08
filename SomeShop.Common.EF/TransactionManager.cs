using Microsoft.EntityFrameworkCore;
using SomeShop.Common.App;

namespace SomeShop.Common.EF;

public class TransactionManager<TDbContext> : ITransactionManager where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    private bool _isCommitted;
    
    public TransactionManager(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task BeginTransaction(CancellationToken cancellationToken = default)
    {
        return _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public bool IsTransactionBegun()
    {
        return _dbContext.Database.CurrentTransaction != default;
    }

    public async Task ExecuteInTransaction(Func<Task> exec, CancellationToken cancellationToken)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await exec();
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    } 

    public async Task Commit(CancellationToken cancellationToken)
    {
        var transaction = _dbContext.Database.CurrentTransaction ??
                          await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await transaction.CommitAsync(cancellationToken);
            _isCommitted = true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isCommitted && _dbContext.Database.CurrentTransaction != default)
        {
            await _dbContext.Database.CurrentTransaction.RollbackAsync().ConfigureAwait(false);
        }
    }
}