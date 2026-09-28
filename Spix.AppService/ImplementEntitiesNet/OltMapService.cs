using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceEntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;
using Spix.xNetwork.MapHelper;

namespace Spix.AppService.ImplementEntitiesNet;

//Mapa de OLT: el gemelo del Mapa de nodos, para la fibra. Sin mascara de cobertura, que es
//del sector inalambrico y aqui no significa nada; en su lugar lleva los puertos del equipo.
//Solo lectura, controlador propio y todo filtrado por la corporacion del usuario.
public class OltMapService : IOltMapService
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IEnumMultilLanguageService _enumMultilLanguageService;

    public OltMapService(
        DataContext context,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer,
        IEnumMultilLanguageService enumMultilLanguageService)
    {
        _context = context;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _enumMultilLanguageService = enumMultilLanguageService;
    }

    //OLT activas: "Nombre · IP", con el neutro traducido en la posicion 0.
    //Ese neutro es el que deja el mapa en la vista de todas las OLT.
    public async Task<ActionResponse<IEnumerable<GuidNameModel>>> ComboOltsAsync(string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<GuidNameModel>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var list = await _context.Olts
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId && x.Active)
                .OrderBy(x => x.OltName)
                .Select(x => new GuidNameModel
                {
                    Value = x.OltId,
                    Name = x.OltName + " · " + x.IpNetwork!.Ip
                })
                .ToListAsync();

            list.Insert(0, new GuidNameModel
            {
                Value = Guid.Empty,
                Name = _localizer["OltMap_AllOlts"]
            });

            return Success<IEnumerable<GuidNameModel>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<GuidNameModel>>(ex);
        }
    }

    //Las formas de ver el mapa, traducidas: solo puntos, lineas, o lineas con distancia
    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboViewsAsync()
    {
        try
        {
            var list = _enumMultilLanguageService.GetEnumSelectList<OltMapViewType>();

            return Success<IEnumerable<IntItemModel>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex);
        }
    }

    //Todas las OLT con su nombre y cuantos clientes tiene cada una. El conteo lo hace la base
    //de una pasada: con cientos de OLT no se trae ningun contrato al servidor.
    public async Task<ActionResponse<IEnumerable<OltMapItemDto>>> GetAllAsync(string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<OltMapItemDto>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var list = await _context.Olts
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId && x.Active)
                .OrderBy(x => x.OltName)
                .Select(x => new OltMapItemDto
                {
                    OltId = x.OltId,
                    OltName = x.OltName,
                    Ip = x.IpNetwork!.Ip,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    Clients = x.ContractOlts!.Count(c => c.ContractClient!.CorporationId == corporationId)
                })
                .ToListAsync();

            return Success<IEnumerable<OltMapItemDto>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<OltMapItemDto>>(ex);
        }
    }

    //La OLT, sus clientes con y sin ubicacion, y el tablero, en una sola respuesta
    public async Task<ActionResponse<OltMapDto>> GetAsync(Guid oltId, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<OltMapDto>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            //La OLT
            var map = await _context.Olts
                .AsNoTracking()
                .Where(x => x.OltId == oltId && x.CorporationId == corporationId)
                .Select(x => new OltMapDto
                {
                    OltId = x.OltId,
                    OltName = x.OltName,
                    Ip = x.IpNetwork!.Ip,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    PortCount = x.PortCount,
                    PortSpeed = x.PortSpeed
                })
                .FirstOrDefaultAsync();
            if (map == null) return Fail<OltMapDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            //Sus clientes, con la ubicacion del contrato si la tiene
            var clients = await _context.ContractOlts
                .AsNoTracking()
                .Where(x => x.OltId == oltId && x.ContractClient!.CorporationId == corporationId)
                .Select(x => new
                {
                    x.ContractClientId,
                    x.ContractClient!.ControlContrato,
                    ClientName = x.ContractClient.Client!.FirstName + " " + x.ContractClient.Client.LastName,
                    Latitude = x.ContractClient.ContractMaps!.Select(m => m.Latitude).FirstOrDefault(),
                    Longitude = x.ContractClient.ContractMaps!.Select(m => m.Longitude).FirstOrDefault()
                })
                .ToListAsync();

            //Con ubicacion: se mide la distancia a la OLT y se ordenan del mas cercano al mas lejano
            var oltHasPoint = map.Latitude.HasValue && map.Longitude.HasValue;
            map.Located = clients
                .Where(x => x.Latitude.HasValue && x.Longitude.HasValue)
                .Select(x => new OltMapClientDto
                {
                    ContractClientId = x.ContractClientId,
                    ControlContrato = x.ControlContrato,
                    ClientName = x.ClientName,
                    Latitude = x.Latitude!.Value,
                    Longitude = x.Longitude!.Value,
                    DistanceKm = oltHasPoint
                        ? Math.Round(GeoDistance.Kilometers(map.Latitude!.Value, map.Longitude!.Value, x.Latitude.Value, x.Longitude.Value), 2)
                        : null
                })
                .OrderBy(x => x.DistanceKm ?? 0)
                .ThenBy(x => x.ClientName)
                .ToList();

            var unlocated = clients
                .Where(x => !x.Latitude.HasValue || !x.Longitude.HasValue)
                .OrderBy(x => x.ClientName)
                .ToList();

            //El tablero
            map.Clients = clients.Count;
            map.WithLocation = map.Located.Count;
            map.WithoutLocation = unlocated.Count;
            map.FarthestKm = map.Located.Max(x => x.DistanceKm);

            //El combo de clientes sin ubicacion, listo para pintar
            map.UnlocatedOptions = unlocated
                .Select(x => new GuidNameModel
                {
                    Value = x.ContractClientId,
                    Name = $"#{x.ControlContrato} {x.ClientName}"
                })
                .ToList();
            map.UnlocatedOptions.Insert(0, new GuidNameModel { Value = Guid.Empty, Name = _localizer["NodeMap_SelectUnlocated", map.WithoutLocation] });

            return Success(map);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<OltMapDto>(ex);
        }
    }

    private async Task<int?> GetCorporationIdAsync(string username)
    {
        var user = await _userHelper.GetUserByUserNameAsync(username);
        return user?.CorporationId;
    }

    private static ActionResponse<T> Success<T>(T result) => new() { WasSuccess = true, Result = result };

    private static ActionResponse<T> Fail<T>(string message) => new() { WasSuccess = false, Message = message };
}
