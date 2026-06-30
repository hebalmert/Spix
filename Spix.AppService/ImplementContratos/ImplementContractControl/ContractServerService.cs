using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppInfra.Validations;
using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementEntitiesNet;

public class ContractServerService : IContractServerService
{
    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;

    public ContractServerService(DataContext context,
        ITransactionManager transactionManager,
        IUserHelper userHelper,
        IStringLocalizer localizer,
        HttpErrorHandler httpErrorHandler)
    {
        _context = context;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _localizer = localizer;
        _httpErrorHandler = httpErrorHandler;
    }

    public async Task<ActionResponse<ContractServer>> GetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return new ActionResponse<ContractServer>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        try
        {
            var modelo = await _context.ContractServers
                .Include(x => x.Server)
                    .ThenInclude(x => x!.IpNetwork)
                .FirstOrDefaultAsync(c => c.ContractClientId == id);

            if (modelo is null)
            {
                modelo = new();
            }

            return new ActionResponse<ContractServer>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractServer>(ex);
        }
    }

    public async Task<ActionResponse<ContractServer>> AddAsync(ContractServer modelo, string username)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<ContractServer>
            {
                WasSuccess = false,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<ContractServer>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            _context.ContractServers.Add(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractServer>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractServer>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var dataRemove = await _context.ContractServers.FindAsync(id);
            if (dataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            var hasHotSpotDependencies = await _context.ContractQues.AnyAsync(x => x.ContractClientId == dataRemove.ContractClientId)
                || await _context.ContractBinds.AnyAsync(x => x.ContractClientId == dataRemove.ContractClientId);
            if (hasHotSpotDependencies)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Debe eliminar Queues Velocidad e IpBinding Acceso antes de cambiar el servidor del contrato."
                };
            }

            _context.ContractServers.Remove(dataRemove);

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
