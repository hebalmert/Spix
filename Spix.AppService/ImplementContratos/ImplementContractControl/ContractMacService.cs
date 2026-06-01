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
using Spix.xNetwork.MacHelper;

namespace Spix.AppService.ImplementEntitiesNet;

public class ContractMacService : IContractMacService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IMapperService _mapperService;
    private readonly IStringLocalizer _localizer;
    private readonly IMacControl _macControl;
    private readonly HttpErrorHandler _httpErrorHandler;

    public ContractMacService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IUserHelper userHelper, HttpErrorHandler httpErrorHandler,
        IMapperService mapperService, IStringLocalizer localizer, IMacControl macControl)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _mapperService = mapperService;
        _localizer = localizer;
        _macControl = macControl;
        _httpErrorHandler = httpErrorHandler;
    }

    public async Task<ActionResponse<ContractMac>> GetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return new ActionResponse<ContractMac>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        try
        {
            var modelo = await _context.ContractMacs
                .Include(x => x.CargueDetail)
                .FirstOrDefaultAsync(c => c.ContractClientId == id);
            //como la consulta da null, entonces igualamos el modelo a New(), para evitar fallos en el Front.
            if (modelo is null)
            {
                modelo = new();
                return new ActionResponse<ContractMac>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }

            return new ActionResponse<ContractMac>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractMac>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<ContractMac>> AddAsync(ContractMac modelo, string username)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<ContractMac>
            {
                WasSuccess = false,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        var contractClient = await _context.ContractClients
            .AsNoTracking()
            .Include(x => x.Client)
            .FirstOrDefaultAsync(c => c.ContractClientId == modelo.ContractClientId);
        var FullNameClient = $"{contractClient?.Client!.FirstName} {contractClient?.Client!.LastName}";

        await _transactionManager.BeginTransactionAsync();
        var transaction = _transactionManager.GetCurrentTransaction();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<ContractMac>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }
            var resulMac = await _macControl.SelectMacWhenAdd(modelo.CargueDetailId, FullNameClient, transaction!);
            if (!resulMac.WasSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractMac>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_Error_Ip_Add)]
                };
            }
            _context.ContractMacs.Add(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractMac>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractMac>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();
        var transaction = _transactionManager.GetCurrentTransaction();

        try
        {

            var DataRemove = await _context.ContractMacs.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }
            _context.ContractMacs.Remove(DataRemove);

            var resultIp = await _macControl.SelectMacToDelete(DataRemove.CargueDetailId, transaction!);
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