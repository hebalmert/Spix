using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppInfra.Validations;
using Spix.AppService.InterfaceEntitiesNet;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;
using Spix.xNetwork.IpHelper;

namespace Spix.AppService.ImplementEntitiesNet;

public class ServerService : IServerService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IMapperService _mapperService;
    private readonly IIpControl _ipControl;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;

    public ServerService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IUserHelper userHelper, HttpErrorHandler httpErrorHandler,
        IMapperService mapperService, IIpControl ipControl, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _mapperService = mapperService;
        _ipControl = ipControl;
        _localizer = localizer;
        _httpErrorHandler = httpErrorHandler;
    }

    public async Task<ActionResponse<IEnumerable<Server>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<Server>>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var queryable = _context.Servers
                .Include(x => x.IpNetwork)
                .Include(x => x.Zone)
                .Where(x => x.CorporationId == user.CorporationId)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.ServerName!.ToLower().Contains(pagination.Filter.ToLower()));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.ServerName).Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<Server>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Server>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Server>> GetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return new ActionResponse<Server>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        try
        {
            var modelo = await _context.Servers.FindAsync(id);
            var ZoneDetail = await _context.Zones.FirstOrDefaultAsync(x => x.ZoneId == modelo!.ZoneId);
            modelo!.StateId = ZoneDetail!.StateId;
            modelo.CityId = ZoneDetail.CityId;
            if (modelo == null)
            {
                return new ActionResponse<Server>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_InvalidId)]
                };
            }

            return new ActionResponse<Server>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Server>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Server>> UpdateAsync(Server modelo)
    {
        if (modelo == null || modelo.MarkId == Guid.Empty)
        {
            return new ActionResponse<Server>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        var transaction = _transactionManager.GetCurrentTransaction();

        try
        {
            //Implementando el Mapeo de Modelos con Mapster
            Server NuevoModelo = _mapperService.Map<Server, Server>(modelo);

            var resultIp = await _ipControl.SelectIpWhenUpdateServer(NuevoModelo.IpNetworkId, NuevoModelo.ServerId, NuevoModelo.ServerName, transaction!);
            if (!resultIp.WasSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<Server>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_Error_Ip_Update)]
                };
            }

            _context.Servers.Update(NuevoModelo);

            await _transactionManager.SaveChangesAsync(); ;
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Server>
            {
                WasSuccess = true,
                Result = modelo
            };

        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Server>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Server>> AddAsync(Server modelo, string username)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<Server>
            {
                WasSuccess = false,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        var transaction = _transactionManager.GetCurrentTransaction();

        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<Server>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }
            var resultIp = await _ipControl.SelectIpWhenAdd(modelo.IpNetworkId, modelo.ServerName, transaction!);
            if (!resultIp.WasSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<Server>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_Error_Ip_Add)]
                };
            }

            modelo.CorporationId = Convert.ToInt32(user.CorporationId);
            _context.Servers.Add(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Server>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Server>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();
        var transaction = _transactionManager.GetCurrentTransaction();

        try
        {

            var DataRemove = await _context.Servers.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }
            _context.Servers.Remove(DataRemove);

            var resultIp = await _ipControl.SelectIpToDelete(DataRemove.IpNetworkId, transaction!);
            if (!resultIp.WasSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_Error_Ip_Delete)]
                };
            }

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