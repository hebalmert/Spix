using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Spix.xLanguage.Resources;

namespace Spix.AppInfra.Transactions;

public class TransactionManager : ITransactionManager
{
    private readonly DataContext _context;
    private readonly IStringLocalizer _localizer;
    private IDbContextTransaction? _transaction;

    public TransactionManager(DataContext context, IStringLocalizer localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
            throw new InvalidOperationException(_localizer[nameof(Resource.Transaction_AlreadyActive)]);

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException(_localizer[nameof(Resource.Transaction_NoActiveToCommit)]);

        await _transaction.CommitAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException(_localizer[nameof(Resource.Transaction_NoActiveToRollback)]);

        await _transaction.RollbackAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public IDbContextTransaction? GetCurrentTransaction()
    {
        return _transaction;
    }


    public void Dispose()
    {
        _transaction?.Dispose();
    }
}