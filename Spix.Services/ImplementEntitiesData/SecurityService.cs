using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.Validations;
using Spix.Domain.EntitiesData;
using Spix.Domain.Enum;
using Spix.Domain.Resources;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesEntitiesData;

namespace Spix.Services.ImplementEntitiesData;

public class SecurityService : ISecurityService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public SecurityService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, HttpErrorHandler httpErrorHandler, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync()
    {
        try
        {
            List<IntItemModel> ListModel = await _context.Securities.Where(x => x.Active)
                .Select(x => new IntItemModel { Name = x.SecurityName, Value = x.SecurityId }).ToListAsync();
            // Insertar el elemento neutro al inicio
            var defaultItem = new IntItemModel
            {
                Value = 0,
                Name = _localizer[nameof(Resource.Select_B_Secutiry)]
            };
            ListModel.Insert(0, defaultItem);

            return new ActionResponse<IEnumerable<IntItemModel>>
            {
                WasSuccess = true,
                Result = ListModel
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IEnumerable<Security>>> GetAsync(PaginationDTO pagination)
    {
        try
        {
            var queryable = _context.Securities.AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.SecurityName!.ToLower().Contains(pagination.Filter.ToLower()));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.SecurityName).Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<Security>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Security>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Security>> GetAsync(int id)
    {
        try
        {
            var modelo = await _context.Securities.FindAsync(id);
            if (modelo == null)
            {
                return new ActionResponse<Security>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_RegisterNotFound)]
                };
            }

            return new ActionResponse<Security>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Security>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Security>> UpdateAsync(Security modelo)
    {
        if (modelo == null || modelo.SecurityId <= 0)
        {
            return new ActionResponse<Security>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }
        await _transactionManager.BeginTransactionAsync();
        try
        {
            _context.Securities.Update(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Security>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Security>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Security>> AddAsync(Security modelo)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<Security>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            _context.Securities.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Security>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Security>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(int id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.Securities.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.Securities.Remove(DataRemove);

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