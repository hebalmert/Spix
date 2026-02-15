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
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.Services.ImplementEntties;

public class StateService : IStateService
{
    private readonly DataContext _context;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IStringLocalizer _localizer;

    public StateService(DataContext context, HttpErrorHandler httpErrorHandler,
        IHttpContextAccessor httpContextAccessor, ITransactionManager transactionManager,
        IStringLocalizer localizer)
    {
        _context = context;
        _httpErrorHandler = httpErrorHandler;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<State>>> ComboAsync(ClaimsDTOs claimsDTOs)
    {
        try
        {
            int IdCountry = await _context.Corporations
                    .Where(c => c.CorporationId == claimsDTOs!.CorporationId)
                    .Select(c => c.CountryId)
                    .FirstOrDefaultAsync();

            IEnumerable<State> ListModel = await _context.States.Where(x => x.CountryId == IdCountry).ToListAsync();
            return new ActionResponse<IEnumerable<State>>
            {
                WasSuccess = true,
                Result = ListModel
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<State>>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<State>>> GetAsync(PaginationDTO pagination)
    {
        try
        {
            //pagination.Id == Trae el ID del Country
            var queryable = _context.States.Where(x => x.CountryId == pagination.Id).AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                //Busqueda grandes mateniendo los indices de los campos, campo Esta Collation CI para Case Insensitive
                queryable = queryable.Where(u => EF.Functions.Like(u.Name, $"%{pagination.Filter}%"));
            }
            var result = await queryable.ApplyFullPaginationAsync(_httpContextAccessor.HttpContext!, pagination);

            return new ActionResponse<IEnumerable<State>>
            {
                WasSuccess = true,
                Result = result
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<State>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<State>> GetAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return new ActionResponse<State>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_InvalidId)]
                };
            }
            var modelo = await _context.States
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.StateId == id);
            if (modelo == null)
            {
                return new ActionResponse<State>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }
            return new ActionResponse<State>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<State>(ex);
        }
    }

    public async Task<ActionResponse<State>> UpdateAsync(State modelo)
    {
        if (modelo == null || modelo.StateId <= 0)
        {
            return new ActionResponse<State>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            _context.States.Update(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<State>
            {
                WasSuccess = true,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_Success)]
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<State>(ex);
        }
    }

    public async Task<ActionResponse<State>> AddAsync(State modelo)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<State>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            _context.States.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<State>
            {
                WasSuccess = true,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_Success)]
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<State>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.States.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.States.Remove(DataRemove);

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
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }
}