using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Spix.Domain.Resources;

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
            throw new InvalidOperationException(_localizer[nameof(Resource.Context_Begin)]);

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException(_localizer[nameof(Resource.Context_Commit)]);

        await _context.SaveChangesAsync();
        await _transaction.CommitAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException(_localizer[nameof(Resource.Context_RollBack)]);

        await _transaction.RollbackAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}