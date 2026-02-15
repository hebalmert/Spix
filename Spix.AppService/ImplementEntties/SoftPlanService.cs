using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.Validations;
using Spix.AppService.InterfaceEntities;
using Spix.Domain.Entities;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.Services.ImplementEntties;

public class SoftPlanService : ISoftPlanService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public SoftPlanService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer, IStringLocalizer localizerDisplay)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<SoftPlan>>> ComboAsync()
    {
        try
        {
            List<SoftPlan> ListModel = await _context.SoftPlans.Where(x => x.Active).ToListAsync();
            // Insertar el elemento neutro al inicio
            var defaultItem = new SoftPlan
            {
                SoftPlanId = 0,
                Name = _localizer[nameof(Resource.Select_Plan)],
                Meses = 0,
                Price = 0,
                ClientsCount = 0,
                Active = true
            };
            ListModel.Insert(0, defaultItem);

            return new ActionResponse<IEnumerable<SoftPlan>>
            {
                WasSuccess = true,
                Result = ListModel
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<SoftPlan>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IEnumerable<SoftPlan>>> GetAsync(PaginationDTO pagination)
    {
        try
        {
            var queryable = _context.SoftPlans.AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                //Busqueda grandes mateniendo los indices de los campos, campo Esta Collation CI para Case Insensitive
                queryable = queryable.Where(u => EF.Functions.Like(u.Name, $"%{pagination.Filter}%"));
            }
            var result = await queryable.ApplyFullPaginationAsync(_httpContextAccessor.HttpContext!, pagination);

            return new ActionResponse<IEnumerable<SoftPlan>>
            {
                WasSuccess = true,
                Result = result
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<SoftPlan>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<SoftPlan>> GetAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return new ActionResponse<SoftPlan>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_InvalidId)]
                };
            }
            var modelo = await _context.SoftPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SoftPlanId == id);
            if (modelo == null)
            {
                return new ActionResponse<SoftPlan>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            return new ActionResponse<SoftPlan>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<SoftPlan>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<SoftPlan>> UpdateAsync(SoftPlan modelo)
    {
        if (modelo == null || modelo.SoftPlanId <= 0)
        {
            return new ActionResponse<SoftPlan>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            _context.SoftPlans.Update(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<SoftPlan>
            {
                WasSuccess = true,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_Success)]
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<SoftPlan>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<SoftPlan>> AddAsync(SoftPlan modelo)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<SoftPlan>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            _context.SoftPlans.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<SoftPlan>
            {
                WasSuccess = true,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_Success)]
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<SoftPlan>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(int id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.SoftPlans.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.SoftPlans.Remove(DataRemove);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = _localizer[nameof(Resource.Generic_Success)]
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex); // ✅ Manejo de errores automático
        }
    }
}