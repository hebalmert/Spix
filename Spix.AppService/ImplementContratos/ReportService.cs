using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementContratos;

//Los reportes de pantalla. Van en su propio servicio y su propio controlador: se consultan
//de vez en cuando y no tienen por que cargar los modulos que se usan todo el dia.
//
//Los agrupados los resuelve la base con un GROUP BY: devuelven una fila por zona o por
//servidor, no un contrato por fila.
public class ReportService : IReportService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IEnumMultilLanguageService _enumMultilLanguageService;

    public ReportService(
        DataContext context,
        IHttpContextAccessor httpContextAccessor,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer,
        IEnumMultilLanguageService enumMultilLanguageService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _enumMultilLanguageService = enumMultilLanguageService;
    }

    //Los totales del reporte: cuantos contratos activos y cuanto suman sus planes
    public async Task<ActionResponse<ReportActiveSummaryDto>> GetActiveSummaryAsync(int stateId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ReportActiveSummaryDto>();

            var corporationId = Convert.ToInt32(user.CorporationId);

            //Una sola pasada: los tres numeros salen del mismo recorrido
            var summary = await ActiveQuery(corporationId, stateId)
                .Select(x => new
                {
                    Price = x.ContractPlans!.Select(p => (decimal?)p.Plan!.Price).FirstOrDefault()
                })
                .GroupBy(x => 1)
                .Select(g => new ReportActiveSummaryDto
                {
                    Contracts = g.Count(),
                    MonthlyTotal = g.Sum(x => x.Price ?? 0),
                    WithoutPlan = g.Count(x => x.Price == null)
                })
                .FirstOrDefaultAsync() ?? new ReportActiveSummaryDto();

            return new ActionResponse<ReportActiveSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ReportActiveSummaryDto>(ex);
        }
    }

    //Los contratos activos con su plan y su monto, pagina por pagina
    public async Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetActiveContractsAsync(int stateId, PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ReportActiveContractDto>>();

            var corporationId = Convert.ToInt32(user.CorporationId);

            var queryable = ActiveQuery(corporationId, stateId)
                .Select(x => new ReportActiveContractDto
                {
                    ControlContrato = x.ControlContrato,
                    ClientFullName = x.Client!.FirstName + " " + x.Client.LastName,
                    ZoneName = x.Zone!.ZoneName,
                    ServerName = x.ContractServers!.Select(s => s.Server!.ServerName).FirstOrDefault(),
                    PlanName = x.ContractPlans!.Select(p => p.Plan!.PlanName).FirstOrDefault(),
                    PlanPrice = x.ContractPlans!.Select(p => (decimal?)p.Plan!.Price).FirstOrDefault() ?? 0,
                    IsActive = x.ContractState == ContractState.Active
                });

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.ClientFullName, $"%{filter}%") ||
                    EF.Functions.Like(x.ZoneName!, $"%{filter}%") ||
                    EF.Functions.Like(x.PlanName!, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderBy(x => x.ControlContrato)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ReportActiveContractDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportActiveContractDto>>(ex);
        }
    }

    //Los estados donde la corporacion tiene zonas. La lista se arma completa aqui, con su
    //elemento neutro: la pantalla solo la pinta.
    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboStatesAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<IntItemModel>>();

            var list = await _context.Zones
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId && x.Active)
                .Select(x => new IntItemModel { Value = x.StateId, Name = x.State!.Name })
                .Distinct()
                .OrderBy(x => x.Name)
                .ToListAsync();

            list.Insert(0, new IntItemModel { Value = 0, Name = _localizer["Report_SelectState"] });

            return new ActionResponse<IEnumerable<IntItemModel>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex);
        }
    }

    //Las ciudades de ese estado donde la corporacion tiene zonas
    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboCitiesAsync(int stateId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<IntItemModel>>();

            var list = await _context.Zones
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId && x.Active && x.StateId == stateId)
                .Select(x => new IntItemModel { Value = x.CityId, Name = x.City!.Name })
                .Distinct()
                .OrderBy(x => x.Name)
                .ToListAsync();

            list.Insert(0, new IntItemModel { Value = 0, Name = _localizer["Report_SelectCity"] });

            return new ActionResponse<IEnumerable<IntItemModel>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex);
        }
    }

    //Las zonas de esa ciudad
    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboZonesAsync(int cityId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<GuidItemModel>>();

            var list = await _context.Zones
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId && x.Active && x.CityId == cityId)
                .Select(x => new GuidItemModel { Value = x.ZoneId, Name = x.ZoneName })
                .OrderBy(x => x.Name)
                .ToListAsync();

            list.Insert(0, new GuidItemModel { Value = Guid.Empty, Name = _localizer["Report_SelectZone"] });

            return new ActionResponse<IEnumerable<GuidItemModel>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<GuidItemModel>>(ex);
        }
    }

    //Lo que factura esa zona: cuantos contratos activos y cuanto suman sus planes
    public async Task<ActionResponse<ReportActiveSummaryDto>> GetZoneSummaryAsync(Guid zoneId, int stateId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ReportActiveSummaryDto>();

            var corporationId = Convert.ToInt32(user.CorporationId);

            var summary = await ActiveQuery(corporationId, stateId)
                .Where(x => x.ZoneId == zoneId)
                .Select(x => new
                {
                    Price = x.ContractPlans!.Select(p => (decimal?)p.Plan!.Price).FirstOrDefault()
                })
                .GroupBy(x => 1)
                .Select(g => new ReportActiveSummaryDto
                {
                    Contracts = g.Count(),
                    MonthlyTotal = g.Sum(x => x.Price ?? 0),
                    WithoutPlan = g.Count(x => x.Price == null)
                })
                .FirstOrDefaultAsync() ?? new ReportActiveSummaryDto();

            return new ActionResponse<ReportActiveSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ReportActiveSummaryDto>(ex);
        }
    }

    //Los contratos activos de esa zona, pagina por pagina
    public async Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetZoneContractsAsync(Guid zoneId, int stateId, PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ReportActiveContractDto>>();

            var corporationId = Convert.ToInt32(user.CorporationId);

            var queryable = ActiveQuery(corporationId, stateId)
                .Where(x => x.ZoneId == zoneId)
                .Select(x => new ReportActiveContractDto
                {
                    ControlContrato = x.ControlContrato,
                    ClientFullName = x.Client!.FirstName + " " + x.Client.LastName,
                    ZoneName = x.Zone!.ZoneName,
                    ServerName = x.ContractServers!.Select(s => s.Server!.ServerName).FirstOrDefault(),
                    PlanName = x.ContractPlans!.Select(p => p.Plan!.PlanName).FirstOrDefault(),
                    PlanPrice = x.ContractPlans!.Select(p => (decimal?)p.Plan!.Price).FirstOrDefault() ?? 0,
                    IsActive = x.ContractState == ContractState.Active
                });

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderBy(x => x.ControlContrato)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ReportActiveContractDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportActiveContractDto>>(ex);
        }
    }

    //Los estados del contrato, con Todos al frente. La lista la arma el backend con el
    //traductor de enums: la pantalla solo la pinta.
    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboContractStatesAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<IntItemModel>>();

            //Solo los tres estados que puede tener un contrato ya instalado: borradores,
            //anulados y retirados no cuelgan de ningun equipo.
            var list = _enumMultilLanguageService.GetEnumSelectList<ContractState>()
                .Where(x => EstadosDelReporte.Contains((ContractState)x.Value))
                .ToList();

            //El neutro se reinserta: al filtrar la lista se habia ido con el resto
            list.Insert(0, new IntItemModel { Value = 0, Name = _localizer["Report_AllContracts"] });

            return new ActionResponse<IEnumerable<IntItemModel>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex);
        }
    }

    //Los AP de la corporacion, para elegir uno
    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboNodesAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<GuidItemModel>>();

            var list = await _context.Nodes
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId)
                .Select(x => new GuidItemModel { Value = x.NodeId, Name = x.NodesName })
                .OrderBy(x => x.Name)
                .ToListAsync();

            list.Insert(0, new GuidItemModel { Value = Guid.Empty, Name = _localizer["Report_SelectNode"] });

            return new ActionResponse<IEnumerable<GuidItemModel>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<GuidItemModel>>(ex);
        }
    }

    //Los servidores de la corporacion, para elegir uno
    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboServersAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<GuidItemModel>>();

            var list = await _context.Servers
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId)
                .Select(x => new GuidItemModel { Value = x.ServerId, Name = x.ServerName })
                .OrderBy(x => x.Name)
                .ToListAsync();

            list.Insert(0, new GuidItemModel { Value = Guid.Empty, Name = _localizer["Report_SelectServer"] });

            return new ActionResponse<IEnumerable<GuidItemModel>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<GuidItemModel>>(ex);
        }
    }

    //Lo que genera un AP: cuantos contratos activos cuelgan de el y cuanto suman
    public async Task<ActionResponse<ReportActiveSummaryDto>> GetNodeSummaryAsync(Guid nodeId, int stateId, string username)
    {
        return await SummaryAsync(x => x.ContractNodes!.Any(n => n.NodeId == nodeId), stateId, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetNodeContractsAsync(Guid nodeId, int stateId, PaginationDTO pagination, string username)
    {
        return await ContractsAsync(x => x.ContractNodes!.Any(n => n.NodeId == nodeId), stateId, pagination, username);
    }

    //Lo mismo para un servidor
    public async Task<ActionResponse<ReportActiveSummaryDto>> GetServerSummaryAsync(Guid serverId, int stateId, string username)
    {
        return await SummaryAsync(x => x.ContractServers!.Any(s => s.ServerId == serverId), stateId, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetServerContractsAsync(Guid serverId, int stateId, PaginationDTO pagination, string username)
    {
        return await ContractsAsync(x => x.ContractServers!.Any(s => s.ServerId == serverId), stateId, pagination, username);
    }

    //Los tres reportes de detalle son el mismo, solo cambia por donde se filtra: la zona,
    //el AP o el servidor. Por eso el filtro entra como parametro y no se repite el codigo.
    private async Task<ActionResponse<ReportActiveSummaryDto>> SummaryAsync(
        System.Linq.Expressions.Expression<Func<ContractClient, bool>> filtro, int stateId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ReportActiveSummaryDto>();

            var summary = await ActiveQuery(Convert.ToInt32(user.CorporationId), stateId)
                .Where(filtro)
                .Select(x => new
                {
                    Price = x.ContractPlans!.Select(p => (decimal?)p.Plan!.Price).FirstOrDefault()
                })
                .GroupBy(x => 1)
                .Select(g => new ReportActiveSummaryDto
                {
                    Contracts = g.Count(),
                    MonthlyTotal = g.Sum(x => x.Price ?? 0),
                    WithoutPlan = g.Count(x => x.Price == null)
                })
                .FirstOrDefaultAsync() ?? new ReportActiveSummaryDto();

            return new ActionResponse<ReportActiveSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ReportActiveSummaryDto>(ex);
        }
    }

    private async Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> ContractsAsync(
        System.Linq.Expressions.Expression<Func<ContractClient, bool>> filtro, int stateId, PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ReportActiveContractDto>>();

            var queryable = ActiveQuery(Convert.ToInt32(user.CorporationId), stateId)
                .Where(filtro)
                .Select(x => new ReportActiveContractDto
                {
                    ControlContrato = x.ControlContrato,
                    ClientFullName = x.Client!.FirstName + " " + x.Client.LastName,
                    ZoneName = x.Zone!.ZoneName,
                    ServerName = x.ContractServers!.Select(s => s.Server!.ServerName).FirstOrDefault(),
                    NodeName = x.ContractNodes!.Select(n => n.Node!.NodesName).FirstOrDefault(),
                    PlanName = x.ContractPlans!.Select(p => p.Plan!.PlanName).FirstOrDefault(),
                    PlanPrice = x.ContractPlans!.Select(p => (decimal?)p.Plan!.Price).FirstOrDefault() ?? 0,
                    IsActive = x.ContractState == ContractState.Active
                });

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderBy(x => x.ControlContrato)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ReportActiveContractDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportActiveContractDto>>(ex);
        }
    }

    //La base de los reportes. Con stateId en cero entran todos los contratos; con un
    //estado, solo los de ese estado: asi se ve cuantos hay activos, suspendidos o exentos.
    private IQueryable<ContractClient> ActiveQuery(int corporationId, int stateId) =>
        _context.ContractClients
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        (stateId == 0
                            ? EstadosDelReporte.Contains(x.ContractState)
                            : (int)x.ContractState == stateId));

    //Los unicos estados que tienen sentido en un reporte de servicio instalado
    private static readonly ContractState[] EstadosDelReporte =
    [
        ContractState.Active,
        ContractState.Suspended,
        ContractState.Exempt
    ];

    private ActionResponse<T> AuthFail<T>() => new()
    {
        WasSuccess = false,
        Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
    };
}
