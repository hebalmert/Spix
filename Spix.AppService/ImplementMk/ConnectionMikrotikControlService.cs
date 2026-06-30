using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppInfra.Validations;
using Spix.AppService.InterfacesMk;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesMK;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementMk;

public class ConnectionMikrotikControlService : IConnectionMikrotikControlService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public ConnectionMikrotikControlService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IUserHelper userHelper, HttpErrorHandler httpErrorHandler, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<ConnectionMikrotikControl>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            User user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<ConnectionMikrotikControl>>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var queryable = _context.ConnectionMikrotikControls
                .Include(x => x.Corporation)
                .Where(x => x.CorporationId == user.CorporationId)
                .AsQueryable();

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable
                .OrderBy(x => x.ConnectionMikrotikControlId)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ConnectionMikrotikControl>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ConnectionMikrotikControl>>(ex);
        }
    }

    public async Task<ActionResponse<ConnectionMikrotikControl>> GetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return new ActionResponse<ConnectionMikrotikControl>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        try
        {
            var modelo = await _context.ConnectionMikrotikControls
                .Include(x => x.Corporation)
                .FirstOrDefaultAsync(x => x.ConnectionMikrotikControlId == id);

            if (modelo == null)
            {
                return new ActionResponse<ConnectionMikrotikControl>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            return new ActionResponse<ConnectionMikrotikControl>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ConnectionMikrotikControl>(ex);
        }
    }

    public async Task<ActionResponse<ConnectionMikrotikControl>> UpdateAsync(ConnectionMikrotikControl modelo)
    {
        if (modelo == null || modelo.ConnectionMikrotikControlId == Guid.Empty)
        {
            return new ActionResponse<ConnectionMikrotikControl>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        await _transactionManager.BeginTransactionAsync();

        try
        {
            var data = await _context.ConnectionMikrotikControls.FindAsync(modelo.ConnectionMikrotikControlId);
            if (data == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ConnectionMikrotikControl>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            data.MikrotikControlType = modelo.MikrotikControlType;

            _context.ConnectionMikrotikControls.Update(data);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ConnectionMikrotikControl>
            {
                WasSuccess = true,
                Result = data
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ConnectionMikrotikControl>(ex);
        }
    }

    public async Task<ActionResponse<ConnectionMikrotikControl>> AddAsync(ConnectionMikrotikControl modelo, string username)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<ConnectionMikrotikControl>
            {
                WasSuccess = false,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        await _transactionManager.BeginTransactionAsync();

        try
        {
            User user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ConnectionMikrotikControl>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var exists = await _context.ConnectionMikrotikControls.AnyAsync(x => x.CorporationId == user.CorporationId);
            if (exists)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ConnectionMikrotikControl>
                {
                    WasSuccess = false,
                    Message = "Ya existe una configuracion Mikrotik para esta corporacion."
                };
            }

            modelo.CorporationId = Convert.ToInt32(user.CorporationId);
            _context.ConnectionMikrotikControls.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ConnectionMikrotikControl>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ConnectionMikrotikControl>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var dataRemove = await _context.ConnectionMikrotikControls.FindAsync(id);
            if (dataRemove == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.ConnectionMikrotikControls.Remove(dataRemove);
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
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }
}
