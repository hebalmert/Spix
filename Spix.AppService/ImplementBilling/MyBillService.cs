using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesBilling;
using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementBilling;

//Las facturas del CLIENTE, en su propio servicio y aparte del modulo de facturacion de la
//oficina. No comparte nada con el, asi que no puede danarlo.
//
//La seguridad esta en una sola puerta: el cliente se resuelve del usuario del token, y
//TODA consulta se filtra por ese ClientId y su CorporationId. Un id de factura ajeno no
//devuelve nada, aunque lo escriban a mano en la URL.
//
//Y para no pesar: el listado va paginado y solo trae el encabezado (los totales los suma
//la base); los renglones se piden solo cuando el cliente abre una factura.
public class MyBillService : IMyBillService
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MyBillService(
        DataContext context,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _httpContextAccessor = httpContextAccessor;
    }

    //Mis facturas, de la mas nueva a la mas vieja
    public async Task<ActionResponse<IEnumerable<MyBillItemDto>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var client = await GetLoggedClientAsync(username);
            if (client == null)
                return AuthFail<IEnumerable<MyBillItemDto>>();

            var queryable = MyBillsQuery(client)
                .Select(x => new MyBillItemDto
                {
                    SellId = x.SellId,
                    InvoiceNumber = x.InvoiceNumber,
                    DateSell = x.DateSell,
                    ControlContrato = x.ControlContrato,
                    Paid = x.Paid,
                    DatePaid = x.DatePaid,

                    //Los totales los suma la base: no se traen los renglones al listado
                    Total = x.SellDetails!.Sum(d => d.Price * d.Quantity),
                    Balance = x.CxCBills!.Where(b => !b.Cancelled).Sum(b => b.Balance)
                });

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderByDescending(x => x.DateSell)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<MyBillItemDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<MyBillItemDto>>(ex);
        }
    }

    //Una factura mia con sus renglones. El id se cruza siempre con mi ClientId: si la
    //factura es de otro cliente, aqui no sale nada.
    public async Task<ActionResponse<MyBillDetailDto>> GetAsync(Guid sellId, string username)
    {
        try
        {
            var client = await GetLoggedClientAsync(username);
            if (client == null)
                return AuthFail<MyBillDetailDto>();

            var bill = await MyBillsQuery(client)
                .Where(x => x.SellId == sellId)
                .Select(x => new MyBillDetailDto
                {
                    SellId = x.SellId,
                    InvoiceNumber = x.InvoiceNumber,
                    DateSell = x.DateSell,
                    ControlContrato = x.ControlContrato,
                    Address = x.Address,
                    ZoneName = x.ZoneName,
                    Paid = x.Paid,
                    DatePaid = x.DatePaid,

                    SubTotal = x.SellDetails!.Sum(d => d.UnitPrice * d.Quantity),
                    TotalTax = x.SellDetails!.Sum(d => d.TaxAmount * d.Quantity),
                    Total = x.SellDetails!.Sum(d => d.Price * d.Quantity),
                    Balance = x.CxCBills!.Where(b => !b.Cancelled).Sum(b => b.Balance),

                    Lines = x.SellDetails!
                        .Select(d => new MyBillLineDto
                        {
                            Concept = d.Concept,
                            Origin = d.Origin,
                            Quantity = d.Quantity,
                            UnitPrice = d.UnitPrice,
                            TaxAmount = d.TaxAmount,
                            Total = d.Price * d.Quantity
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (bill == null)
                return Fail<MyBillDetailDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            return new ActionResponse<MyBillDetailDto> { WasSuccess = true, Result = bill };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<MyBillDetailDto>(ex);
        }
    }

    //Lo que debo hoy: dos numeros para la tarjeta del portal
    public async Task<ActionResponse<MyBillSummaryDto>> GetSummaryAsync(string username)
    {
        try
        {
            var client = await GetLoggedClientAsync(username);
            if (client == null)
                return AuthFail<MyBillSummaryDto>();

            //Solo las que le faltan por pagar, que de un cliente son pocas: se pide el
            //saldo de cada una y se suman aqui
            var saldos = await MyBillsQuery(client)
                .Where(x => !x.Paid)
                .Select(x => x.CxCBills!.Where(b => !b.Cancelled).Sum(b => b.Balance))
                .ToListAsync();

            var summary = new MyBillSummaryDto
            {
                Pending = saldos.Count,
                Balance = saldos.Sum()
            };

            return new ActionResponse<MyBillSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<MyBillSummaryDto>(ex);
        }
    }

    //La unica puerta a los datos: mis facturas, sin las anuladas
    private IQueryable<Sell> MyBillsQuery(LoggedClient client)
    {
        return _context.Sells
            .AsNoTracking()
            .Where(x => x.CorporationId == client.CorporationId &&
                        x.ClientId == client.ClientId &&
                        !x.Cancelled);
    }

    //Quien esta entrando: tiene que ser un usuario con rol Client y tener su ficha de
    //cliente activa en la misma corporacion. Si algo de eso no cuadra, no se consulta nada.
    private async Task<LoggedClient?> GetLoggedClientAsync(string username)
    {
        var user = await _userHelper.GetUserByUserNameAsync(username);
        if (user == null)
            return null;

        var isClient = await _context.UserRoleDetails
            .AnyAsync(x => x.UserId == user.Id && x.UserType == UserType.Client);

        if (!isClient)
            return null;

        return await _context.Clients
            .AsNoTracking()
            .Where(x => x.UserName == user.UserName &&
                        x.CorporationId == user.CorporationId &&
                        x.Active)
            .Select(x => new LoggedClient
            {
                ClientId = x.ClientId,
                CorporationId = x.CorporationId
            })
            .FirstOrDefaultAsync();
    }

    private ActionResponse<T> AuthFail<T>() => Fail<T>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

    private static ActionResponse<T> Fail<T>(string message) => new()
    {
        WasSuccess = false,
        Message = message
    };

    //El cliente que esta entrando: no sale del servicio
    private class LoggedClient
    {
        public Guid ClientId { get; set; }

        public int CorporationId { get; set; }
    }
}
