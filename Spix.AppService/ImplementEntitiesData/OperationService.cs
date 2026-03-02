using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.Validations;
using Spix.AppService.InterfacesEntitiesData;
using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementEntitiesData;

public class OperationService : IOperationService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;

    public OperationService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, HttpErrorHandler httpErrorHandler, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _localizer = localizer;
        _httpErrorHandler = httpErrorHandler;
    }

    public async Task<ActionResponse<IEnumerable<Operation>>> ComboAsync()
    {
        try
        {
            var ListModel = await _context.Operations.Where(x => x.Active).ToListAsync();

            return new ActionResponse<IEnumerable<Operation>>
            {
                WasSuccess = true,
                Result = ListModel
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Operation>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IEnumerable<Operation>>> GetAsync(PaginationDTO pagination)
    {
        try
        {
            var queryable = _context.Operations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.OperationName!.ToLower().Contains(pagination.Filter.ToLower()));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.OperationName).Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<Operation>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Operation>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Operation>> GetAsync(int id)
    {
        if (id == 0)
        {
            return new ActionResponse<Operation>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        try
        {
            var modelo = await _context.Operations.FindAsync(id);
            if (modelo == null)
            {
                return new ActionResponse<Operation>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            return new ActionResponse<Operation>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Operation>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Operation>> UpdateAsync(Operation modelo)
    {
        if (modelo == null || modelo.OperationId == 0)
        {
            return new ActionResponse<Operation>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        await _transactionManager.BeginTransactionAsync();

        try
        {
            _context.Operations.Update(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Operation>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Operation>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Operation>> AddAsync(Operation modelo)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<Operation>
            {
                WasSuccess = false,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            _context.Operations.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Operation>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Operation>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(int id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.Operations.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.Operations.Remove(DataRemove);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex); // ✅ Manejo de errores automático
        }
    }
}