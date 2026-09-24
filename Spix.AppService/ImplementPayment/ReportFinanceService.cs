using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Http;
using Spix.AppInfra;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesPayment;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;
using System.Globalization;

namespace Spix.AppService.ImplementPayment;

//Los reportes del dinero de un periodo: que entro, como entro y quien lo recogio.
//
//Van en su propio servicio, aparte de los modulos de cobro, y todo lo resuelve la base con
//agregados: devuelven numeros y una fila por persona, nunca la lista de cobros.
public class ReportFinanceService : IReportFinanceService
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEnumMultilLanguageService _enumMultilLanguageService;

    public ReportFinanceService(
        DataContext context,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer,
        IHttpContextAccessor httpContextAccessor,
        IEnumMultilLanguageService enumMultilLanguageService)
    {
        _context = context;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _httpContextAccessor = httpContextAccessor;
        _enumMultilLanguageService = enumMultilLanguageService;
    }

    //Cuanto entro en el periodo y como: efectivo, tarjeta, transferencia o cruzado con un
    //adelanto. Lo exonerado se muestra aparte porque no es plata que entro.
    public async Task<ActionResponse<ReportCollectionSummaryDto>> GetCollectionSummaryAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ReportCollectionSummaryDto>();

            //Una sola pasada con todo. Se cuentan SOLO los renglones que traen plata:
            //cada nota nace con un renglon en cero, y ese no es un cobro.
            var summary = await Query(Convert.ToInt32(user.CorporationId), pagination)
                .GroupBy(x => 1)
                .Select(g => new ReportCollectionSummaryDto
                {
                    Collections = g.Count(x => x.Payment > 0),
                    Total = g.Sum(x => x.Payment),
                    Cash = g.Sum(x => x.PaymentMode == "Cash" ? x.Payment : 0),
                    Card = g.Sum(x => x.PaymentMode == "Card" ? x.Payment : 0),
                    Transfer = g.Sum(x => x.PaymentMode == "Transfer" ? x.Payment : 0),
                    PrePayment = g.Sum(x => x.PaymentMode == "PrePayment" ? x.Payment : 0),

                    //Lo exonerado si se suma completo: no entro plata, pero salda la nota
                    Discount = g.Sum(x => x.Discount)
                })
                .FirstOrDefaultAsync() ?? new ReportCollectionSummaryDto();

            return new ActionResponse<ReportCollectionSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ReportCollectionSummaryDto>(ex);
        }
    }

    //Quien recogio y cuanto: una fila por persona, de mayor a menor
    public async Task<ActionResponse<IEnumerable<ReportCollectorDto>>> GetCollectorsAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ReportCollectorDto>>();

            var list = await Query(Convert.ToInt32(user.CorporationId), pagination)
                .Where(x => x.Payment > 0)
                .GroupBy(x => x.UsuarioOwner)
                .Select(g => new ReportCollectorDto
                {
                    Name = g.Key ?? string.Empty,
                    Collections = g.Count(),
                    Total = g.Sum(x => x.Payment)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ReportCollectorDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportCollectorDto>>(ex);
        }
    }

    //Las notas emitidas en el periodo: cuanto se cobro, cuanto se recogio y cuanto falta
    public async Task<ActionResponse<ReportNotesSummaryDto>> GetNotesSummaryAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ReportNotesSummaryDto>();

            var corporationId = Convert.ToInt32(user.CorporationId);
            var queryable = _context.CxCBills
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId && !x.Cancelled);

            if (DateTime.TryParse(pagination.DateStart, CultureInfo.InvariantCulture, out var desde))
                queryable = queryable.Where(x => x.DateNote >= desde.Date);

            if (DateTime.TryParse(pagination.DateEnd, CultureInfo.InvariantCulture, out var hasta))
                queryable = queryable.Where(x => x.DateNote <= hasta.Date);

            //Lo emitido y lo que falta salen de la propia nota; lo cobrado es la diferencia
            var summary = await queryable
                .GroupBy(x => 1)
                .Select(g => new ReportNotesSummaryDto
                {
                    Notes = g.Count(),
                    Issued = g.Sum(x => x.Total),
                    Pending = g.Sum(x => x.Balance)
                })
                .FirstOrDefaultAsync() ?? new ReportNotesSummaryDto();

            summary.Collected = summary.Issued - summary.Pending;

            return new ActionResponse<ReportNotesSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ReportNotesSummaryDto>(ex);
        }
    }

    //La cartera viva repartida por lo vieja que es la nota. Todo se cuenta en una sola
    //pasada contra el indice de la nota: no se trae ni una fila a memoria.
    public async Task<ActionResponse<ReportAgingDto>> GetAgingAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ReportAgingDto>();

            var corporationId = Convert.ToInt32(user.CorporationId);

            //Los tres cortes de la escalera: de aqui para atras la nota ya tiene 30, 60 o 90 dias
            var hoy = DateTime.Today;
            var corte30 = hoy.AddDays(-30);
            var corte60 = hoy.AddDays(-60);
            var corte90 = hoy.AddDays(-90);

            var summary = await BalanceQuery(corporationId)
                .GroupBy(x => 1)
                .Select(g => new ReportAgingDto
                {
                    Notes = g.Count(),
                    Balance = g.Sum(x => x.Balance),

                    NotesCurrent = g.Count(x => x.DateNote >= corte30),
                    Current = g.Sum(x => x.DateNote >= corte30 ? x.Balance : 0),

                    Notes30 = g.Count(x => x.DateNote < corte30 && x.DateNote >= corte60),
                    Days30 = g.Sum(x => x.DateNote < corte30 && x.DateNote >= corte60 ? x.Balance : 0),

                    Notes60 = g.Count(x => x.DateNote < corte60 && x.DateNote >= corte90),
                    Days60 = g.Sum(x => x.DateNote < corte60 && x.DateNote >= corte90 ? x.Balance : 0),

                    Notes90 = g.Count(x => x.DateNote < corte90),
                    Days90 = g.Sum(x => x.DateNote < corte90 ? x.Balance : 0)
                })
                .FirstOrDefaultAsync() ?? new ReportAgingDto();

            return new ActionResponse<ReportAgingDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ReportAgingDto>(ex);
        }
    }

    //Los contratos que mas deben. Se pide un tope (20 por defecto) para que la consulta
    //no crezca con la cartera: primero los totales por contrato y despues los nombres.
    public async Task<ActionResponse<IEnumerable<ReportDebtorDto>>> GetTopDebtorsAsync(int top, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ReportDebtorDto>>();

            var corporationId = Convert.ToInt32(user.CorporationId);

            if (top <= 0 || top > 100)
                top = 20;

            //Primer paso: los numeros, agrupados por contrato
            var deudas = await BalanceQuery(corporationId)
                .GroupBy(x => x.ContractClientId)
                .Select(g => new
                {
                    ContractClientId = g.Key,
                    Notes = g.Count(),
                    Balance = g.Sum(x => x.Balance),
                    OldestNote = g.Min(x => x.DateNote)
                })
                .OrderByDescending(x => x.Balance)
                .Take(top)
                .ToListAsync();

            if (deudas.Count == 0)
                return new ActionResponse<IEnumerable<ReportDebtorDto>> { WasSuccess = true, Result = new List<ReportDebtorDto>() };

            //Segundo paso: a esos pocos contratos se les busca el nombre
            var ids = deudas.Select(x => x.ContractClientId).ToList();
            var contratos = await _context.ContractClients
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId && ids.Contains(x.ContractClientId))
                .Select(x => new
                {
                    x.ContractClientId,
                    x.ControlContrato,
                    ClientFullName = x.Client!.FirstName + " " + x.Client.LastName,
                    ZoneName = x.Zone!.ZoneName
                })
                .ToListAsync();

            var hoy = DateTime.Today;
            var list = deudas
                .Select(d =>
                {
                    var contrato = contratos.FirstOrDefault(c => c.ContractClientId == d.ContractClientId);

                    return new ReportDebtorDto
                    {
                        ControlContrato = contrato?.ControlContrato ?? 0,
                        ClientFullName = contrato?.ClientFullName ?? string.Empty,
                        ZoneName = contrato?.ZoneName,
                        Notes = d.Notes,
                        Balance = d.Balance,
                        OldestNote = d.OldestNote,
                        Days = (hoy - d.OldestNote.Date).Days
                    };
                })
                .OrderByDescending(x => x.Balance)
                .ToList();

            return new ActionResponse<IEnumerable<ReportDebtorDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportDebtorDto>>(ex);
        }
    }

    //Lo que se le causo a cada contratista en el periodo y lo que se le queda debiendo.
    //Una fila por contratista, resuelta por la base.
    public async Task<ActionResponse<IEnumerable<ReportContractorCommissionDto>>> GetContractorCommissionsAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ReportContractorCommissionDto>>();

            var corporationId = Convert.ToInt32(user.CorporationId);
            var queryable = _context.ContractorAccountPayables
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId);

            if (DateTime.TryParse(pagination.DateStart, CultureInfo.InvariantCulture, out var desde))
                queryable = queryable.Where(x => x.DateCreated >= desde.Date);

            if (DateTime.TryParse(pagination.DateEnd, CultureInfo.InvariantCulture, out var hasta))
                queryable = queryable.Where(x => x.DateCreated <= hasta.Date.AddDays(1).AddTicks(-1));

            var list = await queryable
                .GroupBy(x => x.ContractorId)
                .Select(g => new
                {
                    ContractorId = g.Key,
                    Commissions = g.Count(),
                    BaseAmount = g.Sum(x => x.BaseAmount),
                    Total = g.Sum(x => x.Total),
                    Balance = g.Sum(x => x.Balance)
                })
                .Join(_context.Contractors,
                      comision => comision.ContractorId,
                      contratista => contratista.ContractorId,
                      (comision, contratista) => new ReportContractorCommissionDto
                      {
                          Name = contratista.FirstName + " " + contratista.LastName,
                          Commissions = comision.Commissions,
                          BaseAmount = comision.BaseAmount,
                          Total = comision.Total,
                          Paid = comision.Total - comision.Balance,
                          Balance = comision.Balance
                      })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ReportContractorCommissionDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportContractorCommissionDto>>(ex);
        }
    }

    //La bitacora del dinero: cada movimiento que quedo anotado, del mas nuevo al mas viejo.
    //Va paginada siempre: esta tabla solo crece.
    public async Task<ActionResponse<IEnumerable<ReportAuditDto>>> GetAuditAsync(int eventType, PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ReportAuditDto>>();

            var corporationId = Convert.ToInt32(user.CorporationId);
            var queryable = _context.PaymentAudits
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId);

            if (DateTime.TryParse(pagination.DateStart, CultureInfo.InvariantCulture, out var desde))
                queryable = queryable.Where(x => x.DateEvent >= desde.Date);

            if (DateTime.TryParse(pagination.DateEnd, CultureInfo.InvariantCulture, out var hasta))
                queryable = queryable.Where(x => x.DateEvent <= hasta.Date.AddDays(1).AddTicks(-1));

            if (eventType > 0)
                queryable = queryable.Where(x => (int)x.EventType == eventType);

            var proyeccion = queryable.Select(x => new ReportAuditDto
            {
                DateEvent = x.DateEvent,
                EventType = (int)x.EventType,
                Total = x.Total,
                Detail = x.Detail,
                UserByName = x.UserByName,
                ClientFullName = x.Client == null ? null : x.Client.FirstName + " " + x.Client.LastName
            });

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                proyeccion = proyeccion.Where(x =>
                    EF.Functions.Like(x.ClientFullName!, $"%{filter}%") ||
                    EF.Functions.Like(x.Detail!, $"%{filter}%") ||
                    EF.Functions.Like(x.UserByName!, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(proyeccion, pagination.RecordsNumber);
            var list = await proyeccion
                .OrderByDescending(x => x.DateEvent)
                .Paginate(pagination)
                .ToListAsync();

            //El nombre del movimiento se traduce aqui: la base solo guarda el numero
            foreach (var item in list)
            {
                item.EventName = _enumMultilLanguageService.GetLocalizedName((PaymentEventType)item.EventType);
            }

            return new ActionResponse<IEnumerable<ReportAuditDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportAuditDto>>(ex);
        }
    }

    //Los tipos de movimiento, con Todos al frente
    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboEventTypesAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<IntItemModel>>();

            var list = _enumMultilLanguageService.GetEnumSelectList<PaymentEventType>();
            list.Insert(0, new IntItemModel { Value = 0, Name = _localizer["Report_AllEvents"] });

            return new ActionResponse<IEnumerable<IntItemModel>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex);
        }
    }

    //Las notas que todavia deben plata
    private IQueryable<CxCBill> BalanceQuery(int corporationId)
    {
        return _context.CxCBills
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId && !x.Cancelled && x.Balance > 0);
    }

    //Los abonos del periodo. Entra por el indice (CorporationId, UserId, DatePayment)
    //cuando se pide por persona, y por corporacion y fecha cuando se pide todo.
    private IQueryable<CxCBillDetail> Query(int corporationId, PaginationDTO pagination)
    {
        var queryable = _context.CxCBillDetails
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId);

        if (DateTime.TryParse(pagination.DateStart, CultureInfo.InvariantCulture, out var desde))
            queryable = queryable.Where(x => x.DatePayment >= desde.Date);

        if (DateTime.TryParse(pagination.DateEnd, CultureInfo.InvariantCulture, out var hasta))
            queryable = queryable.Where(x => x.DatePayment <= hasta.Date);

        return queryable;
    }

    private ActionResponse<T> AuthFail<T>() => new()
    {
        WasSuccess = false,
        Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
    };
}
