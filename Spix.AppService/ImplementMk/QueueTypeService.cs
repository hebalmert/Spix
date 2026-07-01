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

public class QueueTypeService : IQueueTypeService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public QueueTypeService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IUserHelper userHelper, HttpErrorHandler httpErrorHandler, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<QueueType>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            User user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<QueueType>>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var queryable = _context.QueueTypes
                .Include(x => x.Corporation)
                .Where(x => x.CorporationId == user.CorporationId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.TypeName.ToLower().Contains(pagination.Filter.ToLower()));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable
                .OrderBy(x => x.TypeName)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<QueueType>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<QueueType>>(ex);
        }
    }

    public async Task<ActionResponse<QueueType>> GetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return new ActionResponse<QueueType>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        try
        {
            var modelo = await _context.QueueTypes
                .Include(x => x.Corporation)
                .FirstOrDefaultAsync(x => x.QueueTypeId == id);

            if (modelo == null)
            {
                return new ActionResponse<QueueType>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            return new ActionResponse<QueueType>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<QueueType>(ex);
        }
    }

    public async Task<ActionResponse<QueueType>> UpdateAsync(QueueType modelo)
    {
        if (modelo == null || modelo.QueueTypeId == Guid.Empty)
        {
            return new ActionResponse<QueueType>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<QueueType>
            {
                WasSuccess = false,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        await _transactionManager.BeginTransactionAsync();

        try
        {
            var data = await _context.QueueTypes.FindAsync(modelo.QueueTypeId);
            if (data == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<QueueType>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            var validation = await ValidateQueueTypeAsync(modelo, data.CorporationId);
            if (!validation.WasSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return validation;
            }

            data.TypeName = modelo.TypeName;
            data.Down = modelo.Down;
            data.Up = modelo.Up;
            data.Active = modelo.Active;

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<QueueType>
            {
                WasSuccess = true,
                Result = data
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<QueueType>(ex);
        }
    }

    public async Task<ActionResponse<QueueType>> AddAsync(QueueType modelo, string username)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<QueueType>
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
                return new ActionResponse<QueueType>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            modelo.CorporationId = Convert.ToInt32(user.CorporationId);

            var validation = await ValidateQueueTypeAsync(modelo, modelo.CorporationId);
            if (!validation.WasSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return validation;
            }

            _context.QueueTypes.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<QueueType>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<QueueType>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var dataRemove = await _context.QueueTypes.FindAsync(id);
            if (dataRemove == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.QueueTypes.Remove(dataRemove);
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

    private async Task<ActionResponse<QueueType>> ValidateQueueTypeAsync(QueueType modelo, int corporationId)
    {
        if (modelo.Down && modelo.Up)
        {
            return new ActionResponse<QueueType>
            {
                WasSuccess = false,
                Result = modelo,
                Message = "El Queue Type no puede ser Down y Up al mismo tiempo."
            };
        }

        var nameExists = await _context.QueueTypes.AnyAsync(x =>
            x.CorporationId == corporationId &&
            x.QueueTypeId != modelo.QueueTypeId &&
            x.TypeName.ToLower() == modelo.TypeName.ToLower());

        if (nameExists)
        {
            return new ActionResponse<QueueType>
            {
                WasSuccess = false,
                Result = modelo,
                Message = "Ya existe un Queue Type con este nombre."
            };
        }

        return new ActionResponse<QueueType>
        {
            WasSuccess = true,
            Result = modelo
        };
    }
}
