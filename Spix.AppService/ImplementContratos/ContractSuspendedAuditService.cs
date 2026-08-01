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

public class ContractSuspendedAuditService : IContractSuspendedAuditService
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public ContractSuspendedAuditService(
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

    public async Task<ActionResponse<IEnumerable<ContractSuspendedAuditDTO>>> GetAsync(
        DateTime startDate,
        DateTime endDate,
        string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<ContractSuspendedAuditDTO>>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var start = startDate.Date;
            var end = endDate.Date;
            if (end < start)
            {
                return new ActionResponse<IEnumerable<ContractSuspendedAuditDTO>>
                {
                    WasSuccess = false,
                    Message = "La fecha final no puede ser menor que la fecha inicial."
                };
            }

            var endExclusive = end.AddDays(1);
            var audits = await _context.ContractSuspendedAudits
                .AsNoTracking()
                .Include(x => x.ContractClient)
                .Include(x => x.Client)
                .Where(x => x.CorporationId == user.CorporationId &&
                            x.DateModified >= start &&
                            x.DateModified < endExclusive)
                .OrderByDescending(x => x.DateModified)
                .Select(x => new ContractSuspendedAuditDTO
                {
                    ContractSuspendedAuditId = x.ContractSuspendedAuditId,
                    ContractId = x.ContractId,
                    ClientId = x.ClientId,
                    ControlContrato = x.ContractClient!.ControlContrato,
                    ClientDocument = x.Client!.Document,
                    ClientFullName = $"{x.Client.FirstName} {x.Client.LastName}",
                    DateModified = x.DateModified,
                    UserByName = x.UserByName
                })
                .ToListAsync();

            return new ActionResponse<IEnumerable<ContractSuspendedAuditDTO>>
            {
                WasSuccess = true,
                Result = audits
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ContractSuspendedAuditDTO>>(ex);
        }
    }
}
