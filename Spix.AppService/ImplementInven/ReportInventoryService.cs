using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementInven;

//El reporte de los seriales: cuantos equipos hay de cada producto y como estan repartidos
//entre bodega, clientes y danados.
//
//Vive aparte de los modulos de inventario y todo lo resuelve la base con un agregado: lo
//que viaja es una fila por producto, nunca la lista de seriales.
public class ReportInventoryService : IReportInventoryService
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IEnumMultilLanguageService _enumMultilLanguageService;

    public ReportInventoryService(
        DataContext context,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IEnumMultilLanguageService enumMultilLanguageService,
        IStringLocalizer localizer)
    {
        _context = context;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _enumMultilLanguageService = enumMultilLanguageService;
        _localizer = localizer;
    }

    //Los estados del serial, traducidos y con el neutro en la posicion 0: el front solo
    //pinta la lista. El neutro significa TODOS, no "sin elegir".
    public ActionResponse<IEnumerable<IntItemModel>> SerialStatesCombo()
    {
        var list = _enumMultilLanguageService
            .GetEnumSelectList<SerialStateType>(nameof(Resource.Report_SerialStateAll));

        return new ActionResponse<IEnumerable<IntItemModel>> { WasSuccess = true, Result = list };
    }

    //Los totales de la corporacion: se arman con la misma consulta agrupada, que devuelve
    //una fila por producto, asi que sumarlas aqui no cuesta nada.
    public async Task<ActionResponse<ReportSerialSummaryDto>> GetSerialSummaryAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ReportSerialSummaryDto>();

            var productos = await SerialsQuery(Convert.ToInt32(user.CorporationId)).ToListAsync();

            var summary = new ReportSerialSummaryDto
            {
                Products = productos.Count,
                Total = productos.Sum(x => x.Total),
                Available = productos.Sum(x => x.Available),
                Operative = productos.Sum(x => x.Operative),
                Damaged = productos.Sum(x => x.Damaged)
            };

            return new ActionResponse<ReportSerialSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ReportSerialSummaryDto>(ex);
        }
    }

    //Una fila por producto, del que mas equipos tiene al que menos
    public async Task<ActionResponse<IEnumerable<ReportSerialDto>>> GetSerialsAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ReportSerialDto>>();

            var list = await SerialsQuery(Convert.ToInt32(user.CorporationId))
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ReportSerialDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportSerialDto>>(ex);
        }
    }

    //El DETALLE: un serial por fila, con su estado y, si esta instalado, de quien es.
    //
    //El filtro de estado es opcional: sin el salen todos. Se resuelve TODO en la base y
    //se proyecta a la fila que se pinta, para no traer entidades completas.
    public async Task<ActionResponse<IEnumerable<ReportSerialDetailDto>>> GetSerialDetailAsync(
        string username, SerialStateType? estado, Guid? productId, Guid? storageId)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ReportSerialDetailDto>>();

            var corporationId = Convert.ToInt32(user.CorporationId);

            var lista = await _context.CargueDetails
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId &&
                            (estado == null || x.Status == estado) &&
                            (productId == null || x.Cargue!.ProductId == productId) &&
                            (storageId == null || x.ProductStorageId == storageId))
                .Select(x => new ReportSerialDetailDto
                {
                    MacWlan = x.MacWlan ?? string.Empty,
                    ProductName = x.Cargue!.Product!.ProductName,
                    StorageName = x.ProductStorage!.StorageName,
                    StatusName = x.Status == SerialStateType.Disponible ? "Disponible"
                               : x.Status == SerialStateType.Operativo ? "Instalado"
                               : "Averiado",
                    //El cliente sale del contrato donde esta puesto el equipo
                    ClientName = x.ContractMacs!
                        .Select(m => m.ContractClient!.Client!.FirstName + " " + m.ContractClient.Client.LastName)
                        .FirstOrDefault() ?? string.Empty,
                    ContractNumber = x.ContractMacs!
                        .Select(m => m.ContractClient!.ControlContrato.ToString())
                        .FirstOrDefault() ?? string.Empty,
                    CargueNumber = x.Cargue.ControlCargue ?? string.Empty,
                    DateCargue = x.DateCargue,
                    Comment = x.Comment ?? string.Empty
                })
                .OrderBy(x => x.ProductName)
                .ThenBy(x => x.MacWlan)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ReportSerialDetailDto>> { WasSuccess = true, Result = lista };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportSerialDetailDto>>(ex);
        }
    }

    //Los seriales de la corporacion agrupados por producto
    private IQueryable<ReportSerialDto> SerialsQuery(int corporationId)
    {
        return _context.CargueDetails
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId)
            .GroupBy(x => x.Cargue!.Product!.ProductName)
            .Select(g => new ReportSerialDto
            {
                ProductName = g.Key,
                Total = g.Count(),
                Available = g.Count(x => x.Status == SerialStateType.Disponible),
                Operative = g.Count(x => x.Status == SerialStateType.Operativo),
                Damaged = g.Count(x => x.Status == SerialStateType.Averiado)
            });
    }

    private ActionResponse<T> AuthFail<T>() => new()
    {
        WasSuccess = false,
        Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
    };
}
