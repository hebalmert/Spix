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

//Mapa de nodos: un nodo con todos sus clientes y la distancia de cada uno al AP.
//Solo lectura y con su propio controlador, para no cargar los API de Nodos ni de Contratos.
//Todo filtrado por la corporacion del usuario.
public class NodeMapService : INodeMapService
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IEnumMultilLanguageService _enumMultilLanguageService;

    public NodeMapService(
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

    //Nodos activos: "Nombre · IP", con el neutro traducido en la posicion 0
    public async Task<ActionResponse<IEnumerable<GuidNameModel>>> ComboNodesAsync(string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<GuidNameModel>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var list = await _context.Nodes
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId && x.Active)
                .OrderBy(x => x.NodesName)
                .Select(x => new GuidNameModel
                {
                    Value = x.NodeId,
                    Name = x.NodesName + " · " + x.IpNetwork!.Ip
                })
                .ToListAsync();

            list.Insert(0, new GuidNameModel
            {
                Value = Guid.Empty,
                Name = _localizer[nameof(Resource.Select_Node)]
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
            var list = _enumMultilLanguageService.GetEnumSelectList<NodeMapViewType>();

            return Success<IEnumerable<IntItemModel>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex);
        }
    }

    //Las mascaras de cobertura: ninguna, 45 o 90 grados
    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboCoveragesAsync()
    {
        try
        {
            var list = _enumMultilLanguageService.GetEnumSelectList<NodeMapCoverageType>();

            return Success<IEnumerable<IntItemModel>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex);
        }
    }

    //El nodo, sus clientes con y sin ubicacion, y el tablero, en una sola respuesta
    public async Task<ActionResponse<NodeMapDto>> GetAsync(Guid nodeId, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<NodeMapDto>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            //El nodo
            var map = await _context.Nodes
                .AsNoTracking()
                .Where(x => x.NodeId == nodeId && x.CorporationId == corporationId)
                .Select(x => new NodeMapDto
                {
                    NodeId = x.NodeId,
                    NodesName = x.NodesName,
                    Ip = x.IpNetwork!.Ip,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude
                })
                .FirstOrDefaultAsync();
            if (map == null) return Fail<NodeMapDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            //Sus clientes, con la ubicacion del contrato si la tiene (indices: ContractNodes.NodeId y ContractMaps.ContractClientId)
            var clients = await _context.ContractNodes
                .AsNoTracking()
                .Where(x => x.NodeId == nodeId && x.ContractClient!.CorporationId == corporationId)
                .Select(x => new
                {
                    x.ContractClientId,
                    x.ContractClient!.ControlContrato,
                    ClientName = x.ContractClient.Client!.FirstName + " " + x.ContractClient.Client.LastName,
                    Latitude = x.ContractClient.ContractMaps!.Select(m => m.Latitude).FirstOrDefault(),
                    Longitude = x.ContractClient.ContractMaps!.Select(m => m.Longitude).FirstOrDefault()
                })
                .ToListAsync();

            //Con ubicacion: se mide la distancia al nodo y se ordenan del mas cercano al mas lejano
            var nodeHasPoint = map.Latitude.HasValue && map.Longitude.HasValue;
            map.Located = clients
                .Where(x => x.Latitude.HasValue && x.Longitude.HasValue)
                .Select(x => new NodeMapClientDto
                {
                    ContractClientId = x.ContractClientId,
                    ControlContrato = x.ControlContrato,
                    ClientName = x.ClientName,
                    Latitude = x.Latitude!.Value,
                    Longitude = x.Longitude!.Value,
                    DistanceKm = nodeHasPoint
                        ? Math.Round(GeoDistance.Kilometers(map.Latitude!.Value, map.Longitude!.Value, x.Latitude.Value, x.Longitude.Value), 2)
                        : null,
                    BearingDeg = nodeHasPoint
                        ? Bearing(map.Latitude!.Value, map.Longitude!.Value, x.Latitude.Value, x.Longitude.Value)
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
            return await _httpErrorHandler.HandleErrorAsync<NodeMapDto>(ex);
        }
    }

    //Rumbo del nodo hacia el cliente: 0 grados es el norte y crece hacia el este.
    //Sirve para saber si el cliente cae dentro del sector que apunta el transmisor.
    private static double Bearing(decimal nodeLat, decimal nodeLng, decimal clientLat, decimal clientLng)
    {
        var lat1 = (double)nodeLat * Math.PI / 180;
        var lat2 = (double)clientLat * Math.PI / 180;
        var deltaLng = (double)(clientLng - nodeLng) * Math.PI / 180;

        var y = Math.Sin(deltaLng) * Math.Cos(lat2);
        var x = (Math.Cos(lat1) * Math.Sin(lat2)) - (Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(deltaLng));

        var degrees = Math.Atan2(y, x) * 180 / Math.PI;

        return Math.Round((degrees + 360) % 360, 2);
    }

    private async Task<int?> GetCorporationIdAsync(string username)
    {
        var user = await _userHelper.GetUserByUserNameAsync(username);
        return user?.CorporationId;
    }

    private static ActionResponse<T> Success<T>(T result) => new() { WasSuccess = true, Result = result };

    private static ActionResponse<T> Fail<T>(string message) => new() { WasSuccess = false, Message = message };
}
