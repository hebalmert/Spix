using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementContratos;

//Lee la linea de tiempo de un contrato. Un solo lugar del que leer: lo que los modulos
//fueron anotando en ContractAudit, del primer renglon al ultimo.
public class ContractAuditService : IContractAuditService
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public ContractAuditService(
        DataContext context,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer)
    {
        _context = context;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    public async Task<ActionResponse<ContractAuditListDTO>> GetAsync(Guid contractClientId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<ContractAuditListDTO>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            //El encabezado dice de que contrato es la bitacora
            var encabezado = await _context.ContractClients
                .AsNoTracking()
                .Where(x => x.ContractClientId == contractClientId &&
                            x.CorporationId == user.CorporationId)
                .Select(x => new ContractAuditListDTO
                {
                    ContractClientId = x.ContractClientId,
                    ControlContrato = x.ControlContrato,
                    ClientName = x.Client!.FirstName + " " + x.Client.LastName,
                    ClientDocument = x.Client.Document,
                    ContractAddress = x.Address
                })
                .FirstOrDefaultAsync();

            if (encabezado == null)
            {
                return new ActionResponse<ContractAuditListDTO>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            //Del paso mas viejo al mas nuevo: asi se lee la historia
            encabezado.Events = await _context.ContractAudits
                .AsNoTracking()
                .Where(x => x.ContractClientId == contractClientId &&
                            x.CorporationId == user.CorporationId)
                .OrderBy(x => x.DateEvent)
                .Select(x => new ContractAuditDTO
                {
                    ContractAuditId = x.ContractAuditId,
                    DateEvent = x.DateEvent,
                    EventType = x.EventType,
                    Detail = x.Detail,
                    UserByName = x.UserByName,
                    SourceIp = x.SourceIp,
                    ReferenceId = x.ReferenceId
                })
                .ToListAsync();

            return new ActionResponse<ContractAuditListDTO> { WasSuccess = true, Result = encabezado };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractAuditListDTO>(ex);
        }
    }
}
