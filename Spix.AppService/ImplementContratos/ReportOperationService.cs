using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;
using System.Globalization;

namespace Spix.AppService.ImplementContratos;

//Los reportes de la operacion en un periodo: contratos nuevos, servicios hechos y cuales
//son los que mas se repiten.
//
//Servicio propio, aparte de los modulos del dia a dia, y todo resuelto con agregados: lo
//que viaja son numeros y una fila por tipo de servicio.
public class ReportOperationService : IReportOperationService
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public ReportOperationService(
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

    //Los contratos que entraron en el periodo y lo que agregaron a la facturacion.
    //Se cuentan por la fecha del contrato: es la unica fecha que hoy guarda el contrato.
    public async Task<ActionResponse<ReportContractsSummaryDto>> GetContractsSummaryAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ReportContractsSummaryDto>();

            var summary = await ContractsQuery(Convert.ToInt32(user.CorporationId), pagination)
                .Select(x => new
                {
                    Activo = x.ContractState == ContractState.Active,
                    Price = x.ContractPlans!.Select(p => (decimal?)p.Plan!.Price).FirstOrDefault()
                })
                .GroupBy(x => 1)
                .Select(g => new ReportContractsSummaryDto
                {
                    NewContracts = g.Count(),
                    Installed = g.Count(x => x.Activo),
                    MonthlyTotal = g.Sum(x => x.Activo ? (x.Price ?? 0) : 0)
                })
                .FirstOrDefaultAsync() ?? new ReportContractsSummaryDto();

            return new ActionResponse<ReportContractsSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ReportContractsSummaryDto>(ex);
        }
    }

    //Los planes que mas se contrataron en el periodo
    public async Task<ActionResponse<IEnumerable<ReportPlanDto>>> GetTopPlansAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ReportPlanDto>>();

            var corporationId = Convert.ToInt32(user.CorporationId);
            var contratos = ContractsQuery(corporationId, pagination).Select(x => x.ContractClientId);

            var list = await _context.ContractPlans
                .AsNoTracking()
                .Where(x => contratos.Contains(x.ContractClientId))
                .GroupBy(x => x.Plan!.PlanName)
                .Select(g => new ReportPlanDto
                {
                    Name = g.Key,
                    Times = g.Count(),
                    MonthlyTotal = g.Sum(x => x.Plan!.Price)
                })
                .OrderByDescending(x => x.Times)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ReportPlanDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportPlanDto>>(ex);
        }
    }

    //Lo que paso con las solicitudes del periodo: como entraron y como terminaron.
    //Se cuentan por la fecha en que se pidieron, no por la que se atendieron.
    public async Task<ActionResponse<ReportServicesSummaryDto>> GetServicesSummaryAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ReportServicesSummaryDto>();

            var corporationId = Convert.ToInt32(user.CorporationId);

            //Una sola pasada para todos los conteos
            var summary = await RequestedQuery(corporationId, pagination)
                .GroupBy(x => 1)
                .Select(g => new ReportServicesSummaryDto
                {
                    Requested = g.Count(),
                    PhoneResolved = g.Count(x => x.ScheduleStatus == ScheduleStatus.PhoneResolved),
                    Scheduled = g.Count(x => x.ScheduledAtUtc != null),
                    Completed = g.Count(x => x.ScheduleStatus == ScheduleStatus.Completed),
                    Cancelled = g.Count(x => x.ScheduleStatus == ScheduleStatus.Cancelled)
                })
                .FirstOrDefaultAsync() ?? new ReportServicesSummaryDto();

            //Lo cobrado sale de los renglones de las solicitudes ya facturadas
            var facturadas = RequestedQuery(corporationId, pagination)
                .Where(x => x.Billed)
                .Select(x => x.ServiceRequestId);

            summary.Billed = await _context.ServiceRequestDetails
                .AsNoTracking()
                .Where(x => facturadas.Contains(x.ServiceRequestId))
                .SumAsync(x => (decimal?)(x.Price + x.TaxAmount)) ?? 0;

            return new ActionResponse<ReportServicesSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ReportServicesSummaryDto>(ex);
        }
    }

    //Los tecnicos que mas servicios resolvieron en el periodo
    public async Task<ActionResponse<IEnumerable<ReportTechnicianDto>>> GetTopTechniciansAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ReportTechnicianDto>>();

            var corporationId = Convert.ToInt32(user.CorporationId);

            var list = await CompletedQuery(corporationId, pagination)
                .Where(x => x.TechnicianId != null)
                .GroupBy(x => x.Technician!.FirstName + " " + x.Technician.LastName)
                .Select(g => new ReportTechnicianDto
                {
                    Name = g.Key,
                    Services = g.Count()
                })
                .OrderByDescending(x => x.Services)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ReportTechnicianDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportTechnicianDto>>(ex);
        }
    }

    //Como le fue al corte del periodo: cuantos se cortaron, cuantos pagaron por eso y
    //cuantos siguen abajo. Todo sale de la tabla de suspendidos, que ya trae copiada la
    //zona y el monto del plan: ni un join.
    public async Task<ActionResponse<ReportCutOffSummaryDto>> GetCutOffSummaryAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ReportCutOffSummaryDto>();

            var summary = await CutOffQuery(Convert.ToInt32(user.CorporationId), pagination)
                .GroupBy(x => 1)
                .Select(g => new ReportCutOffSummaryDto
                {
                    Cut = g.Count(),
                    CutAmount = g.Sum(x => x.PlanAmount),

                    Recovered = g.Count(x => x.PaymentReceived),
                    RecoveredAmount = g.Sum(x => x.PaymentReceived ? x.PlanAmount : 0),

                    Reactivated = g.Count(x => x.DateReactivated != null),

                    StillDown = g.Count(x => x.DateReactivated == null),
                    StillDownAmount = g.Sum(x => x.DateReactivated == null ? x.PlanAmount : 0)
                })
                .FirstOrDefaultAsync() ?? new ReportCutOffSummaryDto();

            return new ActionResponse<ReportCutOffSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ReportCutOffSummaryDto>(ex);
        }
    }

    //Donde se corto mas y donde respondieron mejor: una fila por zona
    public async Task<ActionResponse<IEnumerable<ReportCutOffZoneDto>>> GetCutOffZonesAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ReportCutOffZoneDto>>();

            var list = await CutOffQuery(Convert.ToInt32(user.CorporationId), pagination)
                .GroupBy(x => x.ZoneName)
                .Select(g => new ReportCutOffZoneDto
                {
                    Name = g.Key ?? string.Empty,
                    Cut = g.Count(),
                    Recovered = g.Count(x => x.PaymentReceived),
                    Amount = g.Sum(x => x.PlanAmount)
                })
                .OrderByDescending(x => x.Cut)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ReportCutOffZoneDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportCutOffZoneDto>>(ex);
        }
    }

    //Los contratos que hoy no estan dando servicio y lo que se dejo de facturar por ellos.
    //Es una foto de como estan hoy, no del periodo: el contrato no guarda la fecha en que
    //se retiro, solo la fecha en que se creo.
    public async Task<ActionResponse<ReportChurnDto>> GetChurnSummaryAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ReportChurnDto>();

            var summary = await ChurnQuery(Convert.ToInt32(user.CorporationId))
                .GroupBy(x => 1)
                .Select(g => new ReportChurnDto
                {
                    Terminated = g.Count(x => x.ContractState == ContractState.Terminated),
                    TerminatedAmount = g.Sum(x => x.ContractState == ContractState.Terminated ? x.PlanAmount : 0),

                    Cancelled = g.Count(x => x.ContractState == ContractState.Cancelled),
                    CancelledAmount = g.Sum(x => x.ContractState == ContractState.Cancelled ? x.PlanAmount : 0),

                    Suspended = g.Count(x => x.ContractState == ContractState.Suspended),
                    SuspendedAmount = g.Sum(x => x.ContractState == ContractState.Suspended ? x.PlanAmount : 0)
                })
                .FirstOrDefaultAsync() ?? new ReportChurnDto();

            return new ActionResponse<ReportChurnDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ReportChurnDto>(ex);
        }
    }

    //En que zonas se esta yendo la gente: retirados y anulados, una fila por zona
    public async Task<ActionResponse<IEnumerable<ReportChurnZoneDto>>> GetChurnZonesAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ReportChurnZoneDto>>();

            var list = await ChurnQuery(Convert.ToInt32(user.CorporationId))
                .Where(x => x.ContractState == ContractState.Terminated || x.ContractState == ContractState.Cancelled)
                .GroupBy(x => x.ZoneName)
                .Select(g => new ReportChurnZoneDto
                {
                    Name = g.Key ?? string.Empty,
                    Contracts = g.Count(),
                    Amount = g.Sum(x => x.PlanAmount)
                })
                .OrderByDescending(x => x.Contracts)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ReportChurnZoneDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportChurnZoneDto>>(ex);
        }
    }

    //Los cortes del periodo: solo los que hizo el corte por mora
    private IQueryable<ContractSuspended> CutOffQuery(int corporationId, PaginationDTO pagination)
    {
        var desde = Desde(pagination);
        var hasta = Hasta(pagination)?.AddDays(1).AddTicks(-1);

        return _context.ContractSuspendeds
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        x.Origin == SuspendedOrigin.Corte &&
                        (desde == null || x.DateSuspended >= desde) &&
                        (hasta == null || x.DateSuspended <= hasta));
    }

    //Los contratos que no estan dando servicio, con el monto de su plan al lado
    private IQueryable<ChurnRow> ChurnQuery(int corporationId)
    {
        return _context.ContractClients
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        (x.ContractState == ContractState.Terminated ||
                         x.ContractState == ContractState.Cancelled ||
                         x.ContractState == ContractState.Suspended))
            .Select(x => new ChurnRow
            {
                ContractState = x.ContractState,
                ZoneName = x.Zone!.ZoneName,
                PlanAmount = x.ContractPlans!.Select(p => (decimal?)p.Plan!.Price).FirstOrDefault() ?? 0
            });
    }

    //Fila de trabajo de las bajas: no sale del servicio
    private class ChurnRow
    {
        public ContractState ContractState { get; set; }

        public string? ZoneName { get; set; }

        public decimal PlanAmount { get; set; }
    }

    //Los contratos creados en el periodo
    private IQueryable<ContractClient> ContractsQuery(int corporationId, PaginationDTO pagination)
    {
        var desde = Desde(pagination);
        var hasta = Hasta(pagination);

        return _context.ContractClients
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        (desde == null || x.DateCreado >= desde) &&
                        (hasta == null || x.DateCreado <= hasta));
    }

    //Las solicitudes pedidas en el periodo
    private IQueryable<ServiceRequest> RequestedQuery(int corporationId, PaginationDTO pagination)
    {
        var desde = Desde(pagination);
        var hasta = Hasta(pagination);

        return _context.ServiceRequests
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        (desde == null || x.CreatedAtUtc >= desde) &&
                        (hasta == null || x.CreatedAtUtc < hasta!.Value.AddDays(1)));
    }

    //Los servicios que mas se repiten: una fila por tipo de servicio, de mayor a menor
    public async Task<ActionResponse<IEnumerable<ReportServiceDto>>> GetTopServicesAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ReportServiceDto>>();

            var corporationId = Convert.ToInt32(user.CorporationId);
            var completadas = CompletedQuery(corporationId, pagination).Select(x => x.ServiceRequestId);

            var list = await _context.ServiceRequestDetails
                .AsNoTracking()
                .Where(x => completadas.Contains(x.ServiceRequestId))
                .GroupBy(x => new { x.ServiceClient!.ServiceName, x.ServiceCategory!.Name })
                .Select(g => new ReportServiceDto
                {
                    Name = g.Key.ServiceName,
                    CategoryName = g.Key.Name,
                    Times = g.Count(),
                    Total = g.Sum(x => x.Price + x.TaxAmount)
                })
                .OrderByDescending(x => x.Times)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ReportServiceDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportServiceDto>>(ex);
        }
    }

    //Las solicitudes terminadas en el periodo: es lo que de verdad se hizo
    private IQueryable<ServiceRequest> CompletedQuery(int corporationId, PaginationDTO pagination)
    {
        var desde = Desde(pagination);
        var hasta = Hasta(pagination);

        return _context.ServiceRequests
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        x.ScheduleStatus == ScheduleStatus.Completed &&
                        x.CompletedAtUtc != null &&
                        (desde == null || x.CompletedAtUtc >= desde) &&
                        (hasta == null || x.CompletedAtUtc < hasta!.Value.AddDays(1)));
    }

    private static DateTime? Desde(PaginationDTO pagination) =>
        DateTime.TryParse(pagination.DateStart, CultureInfo.InvariantCulture, out var fecha) ? fecha.Date : null;

    private static DateTime? Hasta(PaginationDTO pagination) =>
        DateTime.TryParse(pagination.DateEnd, CultureInfo.InvariantCulture, out var fecha) ? fecha.Date : null;

    private ActionResponse<T> AuthFail<T>() => new()
    {
        WasSuccess = false,
        Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
    };
}
