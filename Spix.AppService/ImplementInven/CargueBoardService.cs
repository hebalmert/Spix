using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesInven;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.Services.ImplementInven;

//Solo LECTURA del cargue de seriales: el tablero, el avance y la lista con donde quedo
//instalado cada equipo. Va aparte de CargueService y CargueDetailsService porque esos los
//usan otras pantallas (Seriales, el combo de MAC en Control de Contratos) y no hay por que
//cambiarles lo que devuelven.
//
//Todo se cuenta en la base con COUNT: nada de traer los seriales a memoria para contarlos.
public class CargueBoardService : ICargueBoardService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public CargueBoardService(
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

    //Los cargues con su avance, del mas nuevo al mas viejo
    public async Task<ActionResponse<IEnumerable<CargueListItemDto>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null)
            {
                return AuthFail<IEnumerable<CargueListItemDto>>();
            }

            var queryable = _context.Cargues
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId);

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.ControlCargue!, $"%{filter}%") ||
                    EF.Functions.Like(x.PurchaseDetail!.Purchase!.NroFactura, $"%{filter}%") ||
                    EF.Functions.Like(x.Product!.ProductName, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);

            var list = await queryable
                .OrderByDescending(x => x.DateCargue)
                .Paginate(pagination)
                .Select(x => new CargueListItemDto
                {
                    CargueId = x.CargueId,
                    ControlCargue = x.ControlCargue,
                    DateCargue = x.DateCargue,
                    NroFactura = x.PurchaseDetail!.Purchase!.NroFactura,
                    ProductName = x.Product!.ProductName,
                    CantToUp = x.CantToUp,
                    Uploaded = x.CargueDetails!.Count(),
                    Status = x.Status
                })
                .ToListAsync();

            return new ActionResponse<IEnumerable<CargueListItemDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<CargueListItemDto>>(ex);
        }
    }

    //Los numeros del tablero, sobre todo el inventario de la corporacion
    public async Task<ActionResponse<CargueSummaryDto>> GetSummaryAsync(string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null)
            {
                return AuthFail<CargueSummaryDto>();
            }

            var abiertos = _context.Cargues
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId && x.Status == CargueType.Pendiente);

            //Lo que falta: lo que dice la compra menos lo que ya se subio
            var porSubir = await abiertos.SumAsync(x => (decimal?)x.CantToUp) ?? 0;
            var subidosAbiertos = await abiertos.SumAsync(x => (int?)x.CargueDetails!.Count()) ?? 0;

            var seriales = _context.CargueDetails
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId);

            var summary = new CargueSummaryDto
            {
                PendingCargues = await abiertos.CountAsync(),
                ToUpload = Math.Max(0, (int)porSubir - subidosAbiertos),
                Available = await seriales.CountAsync(x => x.Status == SerialStateType.Disponible),
                Installed = await seriales.CountAsync(x => x.Status == SerialStateType.Operativo),
                Damaged = await seriales.CountAsync(x => x.Status == SerialStateType.Averiado)
            };

            return new ActionResponse<CargueSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<CargueSummaryDto>(ex);
        }
    }

    //El avance de un cargue: cuanto dice la compra, cuanto se subio y como esta cada serial
    public async Task<ActionResponse<CargueProgressDto>> GetProgressAsync(Guid id, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null)
            {
                return AuthFail<CargueProgressDto>();
            }

            var progress = await _context.Cargues
                .AsNoTracking()
                .Where(x => x.CargueId == id && x.CorporationId == corporationId)
                .Select(x => new CargueProgressDto
                {
                    CargueId = x.CargueId,
                    ControlCargue = x.ControlCargue,
                    DateCargue = x.DateCargue,
                    NroFactura = x.PurchaseDetail!.Purchase!.NroFactura,
                    ProductName = x.Product!.ProductName,
                    CantToUp = x.CantToUp,
                    Uploaded = x.CargueDetails!.Count(),
                    Available = x.CargueDetails!.Count(s => s.Status == SerialStateType.Disponible),
                    Installed = x.CargueDetails!.Count(s => s.Status == SerialStateType.Operativo),
                    Damaged = x.CargueDetails!.Count(s => s.Status == SerialStateType.Averiado),
                    Status = x.Status
                })
                .FirstOrDefaultAsync();

            if (progress == null)
            {
                return new ActionResponse<CargueProgressDto>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            return new ActionResponse<CargueProgressDto> { WasSuccess = true, Result = progress };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<CargueProgressDto>(ex);
        }
    }

    //Los seriales de un cargue, con el contrato donde quedo instalado cada uno
    public async Task<ActionResponse<IEnumerable<CargueSerialDto>>> GetSerialsAsync(Guid id, PaginationDTO pagination, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null)
            {
                return AuthFail<IEnumerable<CargueSerialDto>>();
            }

            var queryable = _context.CargueDetails
                .AsNoTracking()
                .Where(x => x.CargueId == id && x.CorporationId == corporationId);

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x => EF.Functions.Like(x.MacWlan!, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);

            var list = await queryable
                .OrderByDescending(x => x.DateCargue)
                .Paginate(pagination)
                .Select(x => new CargueSerialDto
                {
                    CargueDetailId = x.CargueDetailId,
                    MacWlan = x.MacWlan,
                    Comment = x.Comment,
                    Status = x.Status,
                    DateCargue = x.DateCargue,

                    //Donde quedo instalado: el contrato que tiene asignada esta MAC
                    ControlContrato = _context.ContractMacs
                        .Where(m => m.CargueDetailId == x.CargueDetailId)
                        .Select(m => (long?)m.ContractClient!.ControlContrato)
                        .FirstOrDefault(),
                    InstalledClient = _context.ContractMacs
                        .Where(m => m.CargueDetailId == x.CargueDetailId)
                        .Select(m => m.ContractClient!.Client!.FirstName + " " + m.ContractClient.Client.LastName)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return new ActionResponse<IEnumerable<CargueSerialDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<CargueSerialDto>>(ex);
        }
    }

    private async Task<int?> GetCorporationIdAsync(string username)
    {
        var user = await _userHelper.GetUserByUserNameAsync(username);
        return user?.CorporationId;
    }

    private ActionResponse<T> AuthFail<T>() => new()
    {
        WasSuccess = false,
        Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
    };
}
