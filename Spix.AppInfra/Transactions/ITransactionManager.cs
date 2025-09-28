namespace Spix.AppInfra.Transactions;

public interface ITransactionManager
{
    Task BeginTransactionAsync();

    Task CommitTransactionAsync();

    Task RollbackTransactionAsync();

    Task<int> SaveChangesAsync();
}