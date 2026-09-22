using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceEntitiesNet;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;
using Spix.xNetwork.IpHelper;

namespace Spix.AppService.ImplementEntitiesNet;

//Los nodos de acceso de la corporacion. Toda consulta va filtrada por la corporacion del usuario.
//Usuario, clave y frase de seguridad solo los recibe el Administrator: ni el listado ni el
//combo los llevan. Un nodo con contratos no se borra: se inactiva.
public class NodeService : INodeService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IIpControl _ipControl;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;

    public NodeService(
        DataContext context,
        IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IIpControl ipControl,
        IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _ipControl = ipControl;
        _localizer = localizer;
        _httpErrorHandler = httpErrorHandler;
    }

    //Nodos activos para elegir: solo id y nombre. Con id incluye el que ya tiene el contrato.
    public async Task<ActionResponse<IEnumerable<Node>>> ComboAsync(string username, Guid? id = null)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<Node>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var list = await _context.Nodes
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId && (x.Active || x.NodeId == id))
                .OrderBy(x => x.NodesName)
                .Select(x => new Node { NodeId = x.NodeId, NodesName = x.NodesName })
                .ToListAsync();

            if (id == null)
            {
                list.Insert(0, new Node
                {
                    NodeId = Guid.Empty,
                    NodesName = _localizer[nameof(Resource.Select_Node)]
                });
            }

            return Success<IEnumerable<Node>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Node>>(ex);
        }
    }

    //El tablero: nodos activos e inactivos y cuantos contratos salen por ellos
    public async Task<ActionResponse<NetSummaryDto>> GetSummaryAsync(string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<NetSummaryDto>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            //Una sola pasada por el indice de la corporacion para los tres conteos de equipos
            var summary = await _context.Nodes
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId)
                .GroupBy(x => 1)
                .Select(g => new NetSummaryDto
                {
                    Total = g.Count(),
                    Active = g.Count(x => x.Active),
                    Inactive = g.Count(x => !x.Active)
                })
                .FirstOrDefaultAsync() ?? new NetSummaryDto();

            //Los clientes se cuentan por el indice del equipo en la tabla de contratos
            summary.Clients = await _context.ContractNodes.CountAsync(x => x.Node!.CorporationId == corporationId);

            return Success(summary);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<NetSummaryDto>(ex);
        }
    }

    //El listado: por zona y tipo, sin credenciales
    public async Task<ActionResponse<IEnumerable<NodeListItemDto>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<NodeListItemDto>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var queryable = _context.Nodes
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId);

            //Se busca por nombre, zona o IP
            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.NodesName, $"%{filter}%") ||
                    EF.Functions.Like(x.Zone!.ZoneName, $"%{filter}%") ||
                    EF.Functions.Like(x.IpNetwork!.Ip!, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);

            var list = await queryable
                .OrderBy(x => x.Zone!.ZoneName)
                .ThenBy(x => x.NodesName)
                .Paginate(pagination)
                .Select(x => new NodeListItemDto
                {
                    NodeId = x.NodeId,
                    NodesName = x.NodesName,
                    OperationName = x.Operation!.OperationName,
                    ZoneName = x.Zone!.ZoneName,
                    Ip = x.IpNetwork!.Ip,
                    Active = x.Active,
                    Clients = _context.ContractNodes.Count(c => c.NodeId == x.NodeId),
                    Latitude = x.Latitude,
                    Longitude = x.Longitude
                })
                .ToListAsync();

            return Success<IEnumerable<NodeListItemDto>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<NodeListItemDto>>(ex);
        }
    }

    //Para editar. Las credenciales solo van si quien pide es Administrator.
    public async Task<ActionResponse<Node>> GetAsync(Guid id, string username, bool withCredentials)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<Node>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var modelo = await _context.Nodes
                .AsNoTracking()
                .Include(x => x.Zone)
                .FirstOrDefaultAsync(x => x.NodeId == id && x.CorporationId == corporationId);
            if (modelo == null) return Fail<Node>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            //El formulario elige departamento y ciudad a partir de la zona
            modelo.StateId = modelo.Zone!.StateId;
            modelo.CityId = modelo.Zone.CityId;
            modelo.Zone = null;

            if (!withCredentials)
            {
                modelo.Clave = string.Empty;
                modelo.FraseSeguridad = null;
            }

            return Success(modelo);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Node>(ex);
        }
    }

    public async Task<ActionResponse<Node>> AddAsync(Node modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<Node>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            if (string.IsNullOrWhiteSpace(modelo.Clave)) return await FailRollbackAsync<Node>(_localizer["Net_PasswordRequired"]);

            var name = modelo.NodesName.Trim();
            if (await NameExistsAsync(name, modelo.OperationId, corporationId.Value, null)) return await FailRollbackAsync<Node>(_localizer["Node_NameRepeated", name]);

            //La IP tiene que ser de la corporacion y estar libre
            var ipOk = await _ipControl.AssignAsync(modelo.IpNetworkId, null, name, corporationId.Value);
            if (!ipOk) return await FailRollbackAsync<Node>(_localizer["Net_IpNotAvailable"]);

            //Lo que decide el servidor
            var nuevo = new Node { CorporationId = corporationId.Value };
            CopyFields(modelo, nuevo, true);

            //Persistencia
            _context.Nodes.Add(nuevo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            nuevo.Clave = string.Empty;
            return Success(nuevo);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Node>(ex);
        }
    }

    //La clave vacia deja la que ya tenia (el Auxiliar no la recibe, asi que no la manda)
    public async Task<ActionResponse<Node>> UpdateAsync(Node modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<Node>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.Nodes.FirstOrDefaultAsync(x =>
                x.NodeId == modelo.NodeId &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<Node>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var name = modelo.NodesName.Trim();
            if (await NameExistsAsync(name, modelo.OperationId, corporationId.Value, current.NodeId)) return await FailRollbackAsync<Node>(_localizer["Node_NameRepeated", name]);

            //Si cambio la IP se suelta la anterior; si es la misma, solo se actualiza el nombre
            var ipOk = await _ipControl.AssignAsync(modelo.IpNetworkId, current.IpNetworkId, name, corporationId.Value);
            if (!ipOk) return await FailRollbackAsync<Node>(_localizer["Net_IpNotAvailable"]);

            //Mapeo campo por campo
            CopyFields(modelo, current, false);

            //Persistencia
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(new Node { NodeId = current.NodeId, NodesName = current.NodesName });
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Node>(ex);
        }
    }

    //Solo se borra un nodo sin contratos; su IP queda libre
    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.Nodes.FirstOrDefaultAsync(x =>
                x.NodeId == id &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var inUse = await _context.ContractNodes.AnyAsync(x => x.NodeId == id);
            if (inUse) return await FailRollbackAsync<bool>(_localizer["Node_InUse", current.NodesName]);

            //Persistencia
            await _ipControl.ReleaseAsync(current.IpNetworkId, corporationId.Value);
            _context.Nodes.Remove(current);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(true);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    //Los datos que el usuario puede cambiar. Las credenciales vacias no pisan las guardadas.
    private static void CopyFields(Node from, Node to, bool isNew)
    {
        to.NodesName = from.NodesName.Trim();
        to.IpNetworkId = from.IpNetworkId;
        to.OperationId = from.OperationId;
        to.MarkId = from.MarkId;
        to.MarkModelId = from.MarkModelId;
        to.Usuario = from.Usuario;
        to.ZoneId = from.ZoneId;
        to.Latitude = from.Latitude;
        to.Longitude = from.Longitude;
        to.FrecuencyTypeId = from.FrecuencyTypeId;
        to.FrecuencyId = from.FrecuencyId;
        to.ChannelId = from.ChannelId;
        to.SecurityId = from.SecurityId;
        to.Active = from.Active;

        if (isNew || !string.IsNullOrWhiteSpace(from.Clave)) to.Clave = from.Clave;
        if (isNew || !string.IsNullOrWhiteSpace(from.FraseSeguridad)) to.FraseSeguridad = from.FraseSeguridad;
    }

    private async Task<bool> NameExistsAsync(string name, int operationId, int corporationId, Guid? nodeId)
    {
        return await _context.Nodes.AnyAsync(x =>
            x.CorporationId == corporationId &&
            x.OperationId == operationId &&
            x.NodesName == name &&
            x.NodeId != nodeId);
    }

    private async Task<int?> GetCorporationIdAsync(string username)
    {
        var user = await _userHelper.GetUserByUserNameAsync(username);
        return user?.CorporationId;
    }

    private async Task<ActionResponse<T>> FailRollbackAsync<T>(string message)
    {
        await _transactionManager.RollbackTransactionAsync();
        return Fail<T>(message);
    }

    private static ActionResponse<T> Success<T>(T result) => new() { WasSuccess = true, Result = result };

    private static ActionResponse<T> Fail<T>(string message) => new() { WasSuccess = false, Message = message };
}
