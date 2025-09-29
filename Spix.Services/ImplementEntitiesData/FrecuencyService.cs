using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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

public class FrecuencyService : IFrecuencyService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public FrecuencyService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IMemoryCache cache,
        HttpErrorHandler httpErrorHandler, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync(int id)
    {
        try
        {
            List<IntItemModel> ListModel = await _context.Frecuencies.Where(x => x.Active && x.FrecuencyTypeId == id)
                .Select(x => new IntItemModel { Name = Convert.ToString(x.FrecuencyName), Value = x.FrecuencyId }).ToListAsync();
            // Insertar el elemento neutro al inicio
            var defaultItem = new IntItemModel
            {
                Value = 0,
                Name = _localizer[nameof(Resource.Select_B_Frecuency)]
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

    public async Task<ActionResponse<IEnumerable<Frecuency>>> GetAsync(PaginationDTO pagination)
    {
        try
        {
            var queryable = _context.Frecuencies.Where(x => x.FrecuencyTypeId == pagination.Id).AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.FrecuencyName!.ToString().Contains(pagination.Filter));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.FrecuencyName).Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<Frecuency>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Frecuency>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Frecuency>> GetAsync(int id)
    {
        try
        {
            var modelo = await _context.Frecuencies.FindAsync(id);
            if (modelo == null)
            {
                return new ActionResponse<Frecuency>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_RegisterNotFound)]
                };
            }

            return new ActionResponse<Frecuency>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Frecuency>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Frecuency>> UpdateAsync(Frecuency modelo)
    {
        if (modelo == null || modelo.FrecuencyId <= 0)
        {
            return new ActionResponse<Frecuency>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }
        await _transactionManager.BeginTransactionAsync();

        try
        {
            _context.Frecuencies.Update(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Frecuency>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Frecuency>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Frecuency>> AddAsync(Frecuency modelo)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<Frecuency>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }
        await _transactionManager.BeginTransactionAsync();
        try
        {
            _context.Frecuencies.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Frecuency>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Frecuency>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(int id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.Frecuencies.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.Frecuencies.Remove(DataRemove);

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