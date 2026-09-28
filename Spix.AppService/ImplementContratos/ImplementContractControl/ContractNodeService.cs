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

public class ContractNodeService : IContractNodeService
{
    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;

    public ContractNodeService(DataContext context, ITransactionManager transactionManager,
        IUserHelper userHelper, IStringLocalizer localizer, HttpErrorHandler httpErrorHandler)
    {
        _context = context;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _localizer = localizer;
        _httpErrorHandler = httpErrorHandler;
    }

    //Lo que muestra Control de Contratos. Solo contratos de la corporacion del usuario,
    //y sin la clave del equipo: la pantalla no la usa y las operaciones con el MikroTik
    //la leen directo de la base.
    public async Task<ActionResponse<ContractNode>> GetAsync(Guid id, string username)
    {
        if (id == Guid.Empty)
        {
            return new ActionResponse<ContractNode>
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
                return new ActionResponse<ContractNode>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var modelo = await _context.ContractNodes.AsNoTracking()
                .Include(x => x.Node)
                    .ThenInclude(x => x!.IpNetwork)
                .Include(x => x.Node)
                    .ThenInclude(x => x!.Frecuency)
                .Include(x => x.Node)
                    .ThenInclude(x => x!.Channel)
                .FirstOrDefaultAsync(c => c.ContractClientId == id && c.ContractClient!.CorporationId == user.CorporationId);

            if (modelo?.Node != null)
            {
                modelo.Node.Clave = string.Empty;
                modelo.Node.FraseSeguridad = null;
            }

            return new ActionResponse<ContractNode>
            {
                WasSuccess = true,
                Result = modelo ?? new()
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractNode>(ex);
        }
    }

    public async Task<ActionResponse<ContractNode>> AddAsync(ContractNode modelo, string username)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<ContractNode>
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
                return new ActionResponse<ContractNode>
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
                return new ActionResponse<ContractNode>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            var recursoOk = await _context.Nodes.AnyAsync(x =>
                x.NodeId == modelo.NodeId &&
                x.CorporationId == user.CorporationId);

            if (!recursoOk)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractNode>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.ContractNodes.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractNode>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractNode>(ex);
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
            //Con FindAsync(id) cualquiera que conociera el id borraba la pieza de otra empresa.
            var dataRemove = await _context.ContractNodes
                .FirstOrDefaultAsync(x => x.ContractNodeId == id &&
                                          x.ContractClient!.CorporationId == user.CorporationId);
            if (dataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.ContractNodes.Remove(dataRemove);
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
