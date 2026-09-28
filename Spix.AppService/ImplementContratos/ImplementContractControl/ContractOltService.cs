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

namespace Spix.AppService.ImplementContratos.ImplementContractControl;

//El gemelo de ContractNodeService: por que OLT entra este contrato. Es informativo, no le
//habla al equipo. Se usa en los contratos de FIBRA, donde el nodo inalambrico no aplica.
public class ContractOltService : IContractOltService
{
    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;

    public ContractOltService(DataContext context, ITransactionManager transactionManager,
        IUserHelper userHelper, IStringLocalizer localizer, HttpErrorHandler httpErrorHandler)
    {
        _context = context;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _localizer = localizer;
        _httpErrorHandler = httpErrorHandler;
    }

    //Lo que muestra Control de Contratos. Solo contratos de la corporacion del usuario,
    //y sin la clave de la OLT: la pantalla no la usa.
    public async Task<ActionResponse<ContractOlt>> GetAsync(Guid id, string username)
    {
        if (id == Guid.Empty)
        {
            return new ActionResponse<ContractOlt>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<ContractOlt>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var modelo = await _context.ContractOlts.AsNoTracking()
                .Include(x => x.Olt)
                    .ThenInclude(x => x!.IpNetwork)
                .Include(x => x.Olt)
                    .ThenInclude(x => x!.Zone)
                .FirstOrDefaultAsync(c => c.ContractClientId == id && c.ContractClient!.CorporationId == user.CorporationId);

            if (modelo?.Olt != null)
            {
                modelo.Olt.Clave = string.Empty;
            }

            return new ActionResponse<ContractOlt>
            {
                WasSuccess = true,
                Result = modelo ?? new()
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractOlt>(ex);
        }
    }

    public async Task<ActionResponse<ContractOlt>> AddAsync(ContractOlt modelo, string username)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<ContractOlt>
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
                return new ActionResponse<ContractOlt>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            //El contrato tiene que ser de SU corporacion. Recibir el username no alcanza:
            //sin esto, con el id de un contrato ajeno se le agrega una pieza a otra empresa.
            var contratoOk = await _context.ContractClients.AnyAsync(x =>
                x.ContractClientId == modelo.ContractClientId &&
                x.CorporationId == user.CorporationId);

            if (!contratoOk)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractOlt>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            var recursoOk = await _context.Olts.AnyAsync(x =>
                x.OltId == modelo.OltId &&
                x.CorporationId == user.CorporationId);

            if (!recursoOk)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractOlt>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.ContractOlts.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractOlt>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractOlt>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            //Por id SOLO no alcanza: la pieza tiene que ser de un contrato de SU corporacion.
            var dataRemove = await _context.ContractOlts
                .FirstOrDefaultAsync(x => x.ContractOltId == id &&
                                          x.ContractClient!.CorporationId == user.CorporationId);
            if (dataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.ContractOlts.Remove(dataRemove);
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
