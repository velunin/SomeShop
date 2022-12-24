namespace SomeShop.Common.App;

public interface ITransactionManager
{
    bool IsTransactionBegun();
    Task ExecuteInTransaction(Func<Task> exec, CancellationToken cancellationToken);
}