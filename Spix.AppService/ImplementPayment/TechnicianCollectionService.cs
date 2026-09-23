using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
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

//El cruce con el que recoge la plata en la calle: entre dos fechas, que cobro y cuanto.
//
//Va en su propio servicio y su propio controlador para no cargar Cuentas por Cobrar, y
//lee del unico lugar donde vive el dinero: los abonos. Nada se duplica.
public class TechnicianCollectionService : ITechnicianCollectionService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public TechnicianCollectionService(
        DataContext context,
        IHttpContextAccessor httpContextAccessor,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    //Los tecnicos de la corporacion. La lista sale del rol, no de los cobros: asi aparece
    //tambien el que todavia no ha recogido nada y se ve que su periodo va en cero.
    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboCollectorsAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<GuidItemModel>>();

            var tecnicos = await _context.UserRoleDetails
                .AsNoTracking()
                .Where(x => x.UserType == UserType.Technician &&
                            x.User!.CorporationId == user.CorporationId)
                .Select(x => new { x.UserId, x.User!.FirstName, x.User.LastName })
                .ToListAsync();

            //El id del usuario se guarda como texto: se pasa a Guid aqui, que son pocos
            var list = tecnicos
                .Where(x => Guid.TryParse(x.UserId, out _))
                .Select(x => new GuidItemModel
                {
                    Value = Guid.Parse(x.UserId!),
                    Name = $"{x.FirstName} {x.LastName}"
                })
                .OrderBy(x => x.Name)
                .ToList();

            list.Insert(0, new GuidItemModel { Value = Guid.Empty, Name = _localizer["Collection_SelectOne"] });

            return new ActionResponse<IEnumerable<GuidItemModel>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<GuidItemModel>>(ex);
        }
    }

    //El resumen del periodo: es lo que hay que recibirle. Separado por modo, porque el
    //efectivo se entrega en mano y lo demas ya entro al banco.
    public async Task<ActionResponse<TechnicianCollectionSummaryDto>> GetSummaryAsync(Guid userId, PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<TechnicianCollectionSummaryDto>();

            //Una sola pasada, agrupando por modo de pago: devuelve tres filas, no los cobros
            var porModo = await Query(userId, pagination, Convert.ToInt32(user.CorporationId))
                .GroupBy(x => x.PaymentMode)
                .Select(g => new
                {
                    Mode = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(x => x.Payment)
                })
                .ToListAsync();

            var summary = new TechnicianCollectionSummaryDto
            {
                Collections = porModo.Sum(x => x.Count),
                Total = porModo.Sum(x => x.Total),
                Cash = porModo.Where(x => x.Mode == "Cash").Sum(x => x.Total),
                Card = porModo.Where(x => x.Mode == "Card").Sum(x => x.Total),
                Transfer = porModo.Where(x => x.Mode == "Transfer").Sum(x => x.Total)
            };

            return new ActionResponse<TechnicianCollectionSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<TechnicianCollectionSummaryDto>(ex);
        }
    }

    //Los cobros del periodo, pagina por pagina
    public async Task<ActionResponse<IEnumerable<TechnicianCollectionDto>>> GetCollectionsAsync(Guid userId, PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<TechnicianCollectionDto>>();

            var queryable = Query(userId, pagination, Convert.ToInt32(user.CorporationId))
                .Select(x => new TechnicianCollectionDto
                {
                    DatePayment = x.DatePayment,
                    CollectionNote = x.CxCBill!.CollectionNote,
                    ControlContrato = x.CxCBill.ContractClient!.ControlContrato,
                    ClientFullName = x.CxCBill.Client!.FirstName + " " + x.CxCBill.Client.LastName,
                    PaymentMode = x.PaymentMode,
                    Payment = x.Payment,
                    Discount = x.Discount
                });

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderByDescending(x => x.DatePayment)
                .ThenBy(x => x.ControlContrato)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<TechnicianCollectionDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<TechnicianCollectionDto>>(ex);
        }
    }

    //Los abonos de esa persona entre las dos fechas. Entra por el indice
    //(CorporationId, UserId, DatePayment): es un salto directo al pedazo que se pide,
    //asi la tabla tenga millones de filas.
    private IQueryable<CxCBillDetail> Query(Guid userId, PaginationDTO pagination, int corporationId)
    {
        var queryable = _context.CxCBillDetails
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        x.UserId == userId &&
                        x.Payment > 0);

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
