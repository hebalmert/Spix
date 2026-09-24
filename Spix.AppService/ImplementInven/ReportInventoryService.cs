using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
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

    public ReportInventoryService(
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
