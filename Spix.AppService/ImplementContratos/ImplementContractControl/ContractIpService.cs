using DocumentFormat.OpenXml.Office2019.Drawing.Model3D;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppInfra.Validations;
using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;
using Spix.xNetwork.IpHelper;

namespace Spix.AppService.ImplementEntitiesNet;

public class ContractIpService : IContractIpService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IMapperService _mapperService;
    private readonly IIpNetControl _ipControl;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;

    public ContractIpService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IUserHelper userHelper, HttpErrorHandler httpErrorHandler,
        IMapperService mapperService, IIpNetControl ipControl, IStringLocalizer localizer)
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

    public async Task<ActionResponse<ContractIp>> GetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return new ActionResponse<ContractIp>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        try
        {
            var modelo = await _context.ContractIps
                .Include(x => x.IpNet)
                .FirstOrDefaultAsync(c => c.ContractClientId == id);
            //como la consulta da null, entonces igualamos el modelo a New(), para evitar fallos en el Front.
            if (modelo is null)
            {
                modelo = new();
                return new ActionResponse<ContractIp>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }

            return new ActionResponse<ContractIp>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractIp>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<ContractIp>> AddAsync(ContractIp modelo, string username)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<ContractIp>
            {
                WasSuccess = false,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        var contractClient = await _context.ContractClients
            .AsNoTracking()
            .Include(x=> x.Client)
            .FirstOrDefaultAsync(c => c.ContractClientId == modelo.ContractClientId);
        var FullNameClient = $"{contractClient?.Client!.FirstName} {contractClient?.Client!.LastName}";

        await _transactionManager.BeginTransactionAsync();
        var transaction = _transactionManager.GetCurrentTransaction();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<ContractIp>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }
            var resultIp = await _ipControl.SelectIpNetWhenAdd(modelo.IpNetId, FullNameClient, transaction!);
            if (!resultIp.WasSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractIp>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_Error_Ip_Add)]
                };
            }
            _context.ContractIps.Add(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractIp>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractIp>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();
        var transaction = _transactionManager.GetCurrentTransaction();

        try
        {

            var DataRemove = await _context.ContractIps.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }
            _context.ContractIps.Remove(DataRemove);

            var resultIp = await _ipControl.SelectIpNetToDelete(DataRemove.IpNetId, transaction!);
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