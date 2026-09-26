using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementContratos.ImplementContractControl;

// Le entrega al ESCRITORIO los datos que necesita para hablar el mismo con el MikroTik.
//
// Por que existe: el escritorio configura el equipo por la red LAN, porque el cliente
// puede no tener IP publica. Para mandar las mismas ordenes que manda el Backend necesita
// saber lo mismo, y dos de esos datos no estaban expuestos por ningun endpoint: si el
// queue padre ya existe, y que IPs cuelgan de el.
//
// Es de SOLO LECTURA. No crea, no modifica y no toca el equipo: solo junta datos. Vive en
// su propio archivo, aparte de ContractQueService y ContractBindService, para no meterle
// mano a lo que ya funciona en produccion.
public partial class ContractMkSetupService : IContractMkSetupService
{
    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;

    public ContractMkSetupService(
        DataContext context,
        ITransactionManager transactionManager,
        IUserHelper userHelper,
        IStringLocalizer localizer,
        HttpErrorHandler httpErrorHandler)
    {
        _context = context;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _localizer = localizer;
        _httpErrorHandler = httpErrorHandler;
    }

    // Todo lo que hace falta para armar la Queue de un contrato
    public async Task<ActionResponse<ContractQueSetupDTO>> GetQueSetupAsync(Guid contractClientId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return Fail<ContractQueSetupDTO>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            var datos = new ContractQueSetupDTO();

            //Una queue por contrato: si ya esta, no hay nada que armar
            var yaExiste = await _context.ContractQues
                .AnyAsync(x => x.ContractClientId == contractClientId);

            if (yaExiste)
            {
                datos.Blocked = "Ya existe una Queue de Velocidad para este contrato.";
                return Ok(datos);
            }

            //Las piezas que el contrato ya tiene configuradas
            var contrato = await _context.ContractClients
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId &&
                                          x.CorporationId == user.CorporationId);

            var contratoServidor = await _context.ContractServers
                .AsNoTracking()
                .Include(x => x.Server!).ThenInclude(x => x.IpNetwork)
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId);

            var contratoIp = await _context.ContractIps
                .AsNoTracking()
                .Include(x => x.IpNet)
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId);

            var contratoPlan = await _context.ContractPlans
                .AsNoTracking()
                .Include(x => x.Plan)
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId);

            if (contrato == null || contratoServidor?.Server?.IpNetwork?.Ip == null ||
                contratoIp?.IpNet?.Ip == null || contratoPlan?.Plan == null)
            {
                datos.Blocked = "Falta configurar el servidor, la IP o el plan del contrato.";
                return Ok(datos);
            }

            var plan = contratoPlan.Plan;

            if (plan.SpeedUp == null || plan.SpeedDown == null || plan.TasaReuso == null)
            {
                datos.Blocked = "El plan no tiene velocidades o tasa de reuso configuradas.";
                return Ok(datos);
            }

            //Con la tasa en cero el calculo del queue padre divide por cero: se para aqui
            //y se dice que hay que arreglar el plan, en vez de reventar contra el equipo
            if (plan.TasaReuso.Value <= 0)
            {
                datos.Blocked = "El plan tiene la tasa de reuso en cero: corrijala antes de crear la Queue.";
                return Ok(datos);
            }

            var cliente = await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ClientId == contrato.ClientId);

            //Los Queue Types de la corporacion: sin ellos el equipo no sabe que PCQ usar
            var tipos = await _context.QueueTypes
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId)
                .ToListAsync();

            datos.PcqUp = tipos.FirstOrDefault(x => x.Up)?.TypeName;
            datos.PcqDown = tipos.FirstOrDefault(x => x.Down)?.TypeName;

            if (string.IsNullOrWhiteSpace(datos.PcqUp) || string.IsNullOrWhiteSpace(datos.PcqDown))
            {
                datos.Blocked = "Debe configurar un Queue Type Down y un Queue Type Up para esta corporacion.";
                return Ok(datos);
            }

            //El servidor al que se conecta el escritorio, por la LAN
            var servidor = contratoServidor.Server!;

            datos.ServerId = servidor.ServerId;
            datos.ServerName = servidor.ServerName;
            datos.ServerIp = servidor.IpNetwork!.Ip;
            datos.Usuario = servidor.Usuario;
            datos.Clave = servidor.Clave;
            datos.ApiPort = servidor.ApiPort;

            datos.IpNetId = contratoIp.IpNetId;
            datos.IpCliente = contratoIp.IpNet!.Ip;
            datos.NombreCliente = $"{cliente?.FirstName} {cliente?.LastName} - ({contrato.ControlContrato})";

            datos.PlanId = plan.PlanId;
            datos.PlanName = plan.PlanName;
            datos.VelocidadUp = plan.VelocidadUp;
            datos.VelocidadDown = plan.VelocidadDown;
            datos.VelocidadTotal = plan.VelocidadTotal;
            datos.TasaReuso = plan.TasaReuso.Value;

            //En kbps, que es como las pide el equipo
            datos.SpeedUpKbps = plan.SpeedUpType == SpeedUpType.M
                ? plan.SpeedUp.Value * 1024
                : plan.SpeedUp.Value;

            datos.SpeedDownKbps = plan.SpeedDownType == SpeedDownType.M
                ? plan.SpeedDown.Value * 1024
                : plan.SpeedDown.Value;

            //Lo que no se podia pedir por ningun lado: el queue padre de ese servidor y plan
            var padre = await _context.QueueParents
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ServerId == servidor.ServerId && x.PlanId == plan.PlanId);

            datos.HasParent = padre != null;
            datos.ParentMikrotikId = padre?.MkId;
            datos.ParentName = padre?.ParentName;

            //Y las IPs de los clientes que ya cuelgan de ese padre: de ahi salen el target
            //del padre y el calculo de su velocidad
            datos.ClientIps = await (from cn in _context.ContractClients
                                     join pl in _context.ContractPlans on cn.ContractClientId equals pl.ContractClientId
                                     join sv in _context.ContractServers on cn.ContractClientId equals sv.ContractClientId
                                     join ip in _context.ContractIps on cn.ContractClientId equals ip.ContractClientId
                                     join qu in _context.ContractQues on cn.ContractClientId equals qu.ContractClientId
                                     where pl.PlanId == plan.PlanId && sv.ServerId == servidor.ServerId
                                     select ip.IpNet!.Ip!)
                                    .ToListAsync();

            return Ok(datos);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractQueSetupDTO>(ex);
        }
    }

    // Lo que hace falta para armar el IpBinding: con el servidor, la IP y la MAC alcanza
    public async Task<ActionResponse<ContractBindSetupDTO>> GetBindSetupAsync(Guid contractClientId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return Fail<ContractBindSetupDTO>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            var datos = new ContractBindSetupDTO();

            var yaExiste = await _context.ContractBinds
                .AnyAsync(x => x.ContractClientId == contractClientId);

            if (yaExiste)
            {
                datos.Blocked = "Ya existe un IpBinding para este contrato.";
                return Ok(datos);
            }

            var contrato = await _context.ContractClients
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId &&
                                          x.CorporationId == user.CorporationId);

            var contratoServidor = await _context.ContractServers
                .AsNoTracking()
                .Include(x => x.Server!).ThenInclude(x => x.IpNetwork)
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId);

            var contratoIp = await _context.ContractIps
                .AsNoTracking()
                .Include(x => x.IpNet)
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId);

            var contratoMac = await _context.ContractMacs
                .AsNoTracking()
                .Include(x => x.CargueDetail)
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId);

            if (contrato == null || contratoServidor?.Server?.IpNetwork?.Ip == null ||
                contratoIp?.IpNet?.Ip == null || contratoMac?.CargueDetail == null)
            {
                datos.Blocked = "Falta configurar el servidor, la IP o la MAC del contrato.";
                return Ok(datos);
            }

            var cliente = await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ClientId == contrato.ClientId);

            var servidor = contratoServidor.Server!;

            datos.ServerId = servidor.ServerId;
            datos.ServerName = servidor.ServerName;
            datos.ServerIp = servidor.IpNetwork!.Ip;
            datos.Usuario = servidor.Usuario;
            datos.Clave = servidor.Clave;
            datos.ApiPort = servidor.ApiPort;

            datos.IpNetId = contratoIp.IpNetId;
            datos.IpCliente = contratoIp.IpNet!.Ip;

            datos.CargueDetailId = contratoMac.CargueDetailId;
            datos.MacCliente = contratoMac.CargueDetail!.MacWlan;

            datos.NombreCliente = $"{cliente?.FirstName} {cliente?.LastName} - ({contrato.ControlContrato})";

            return Ok(datos);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractBindSetupDTO>(ex);
        }
    }

    // Lo que hace falta para QUITAR la Queue: a quien se le habla, cual queue se borra y
    // como queda el padre cuando este cliente se vaya
    public async Task<ActionResponse<ContractQueRemoveSetupDTO>> GetQueRemoveSetupAsync(Guid contractQueId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return Fail<ContractQueRemoveSetupDTO>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            var datos = new ContractQueRemoveSetupDTO();

            var queue = await _context.ContractQues
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ContractQueId == contractQueId);

            if (queue == null)
            {
                datos.Blocked = _localizer[nameof(Resource.Generic_IdNotFound)];
                return Ok(datos);
            }

            var servidor = await _context.Servers
                .AsNoTracking()
                .Include(x => x.IpNetwork)
                .FirstOrDefaultAsync(x => x.ServerId == queue.ServerId);

            var plan = await _context.Plans
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PlanId == queue.PlanId);

            if (servidor?.IpNetwork?.Ip == null || plan?.SpeedUp == null ||
                plan.SpeedDown == null || plan.TasaReuso == null)
            {
                datos.Blocked = "Falta el servidor o el plan de esta Queue.";
                return Ok(datos);
            }

            datos.ServerId = servidor.ServerId;
            datos.ServerName = servidor.ServerName;
            datos.ServerIp = servidor.IpNetwork!.Ip;
            datos.Usuario = servidor.Usuario;
            datos.Clave = servidor.Clave;
            datos.ApiPort = servidor.ApiPort;

            datos.ContractQueId = queue.ContractQueId;
            datos.MikrotikId = queue.MikrotikId;

            //El plan en kbps, que es como las pide el equipo
            datos.TasaReuso = plan.TasaReuso.Value <= 0 ? 1 : plan.TasaReuso.Value;

            datos.SpeedUpKbps = plan.SpeedUpType == SpeedUpType.M
                ? plan.SpeedUp.Value * 1024
                : plan.SpeedUp.Value;

            datos.SpeedDownKbps = plan.SpeedDownType == SpeedDownType.M
                ? plan.SpeedDown.Value * 1024
                : plan.SpeedDown.Value;

            var padre = await _context.QueueParents
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ServerId == queue.ServerId && x.PlanId == queue.PlanId);

            datos.ParentMkId = padre?.MkId;
            datos.ParentName = padre?.ParentName;

            //Los que QUEDAN: este contrato ya no cuenta, que es justo el que se va
            datos.ClientIps = await (from cn in _context.ContractClients
                                     join ip in _context.ContractIps on cn.ContractClientId equals ip.ContractClientId
                                     join qu in _context.ContractQues on cn.ContractClientId equals qu.ContractClientId
                                     where qu.PlanId == queue.PlanId && qu.ServerId == queue.ServerId &&
                                           qu.ContractQueId != queue.ContractQueId
                                     select ip.IpNet!.Ip!)
                                    .ToListAsync();

            return Ok(datos);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractQueRemoveSetupDTO>(ex);
        }
    }

    // Con quien hablar y como se llama el cliente: alcanza para editar y para quitar el
    // IpBinding, que son las dos ordenes que el escritorio arma sin calcular nada
    public async Task<ActionResponse<ContractMkConnectionDTO>> GetConnectionAsync(Guid contractClientId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return Fail<ContractMkConnectionDTO>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            var datos = new ContractMkConnectionDTO();

            var contrato = await _context.ContractClients
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId &&
                                          x.CorporationId == user.CorporationId);

            var contratoServidor = await _context.ContractServers
                .AsNoTracking()
                .Include(x => x.Server!).ThenInclude(x => x.IpNetwork)
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId);

            if (contrato == null || contratoServidor?.Server?.IpNetwork?.Ip == null)
            {
                datos.Blocked = "El contrato no tiene servidor configurado.";
                return Ok(datos);
            }

            var cliente = await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ClientId == contrato.ClientId);

            var servidor = contratoServidor.Server!;

            datos.ServerId = servidor.ServerId;
            datos.ServerName = servidor.ServerName;
            datos.ServerIp = servidor.IpNetwork!.Ip;
            datos.Usuario = servidor.Usuario;
            datos.Clave = servidor.Clave;
            datos.ApiPort = servidor.ApiPort;
            datos.NombreCliente = $"{cliente?.FirstName} {cliente?.LastName} - ({contrato.ControlContrato})";

            return Ok(datos);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractMkConnectionDTO>(ex);
        }
    }

    private static ActionResponse<T> Ok<T>(T resultado)
    {
        return new ActionResponse<T> { WasSuccess = true, Result = resultado };
    }

    private static ActionResponse<T> Fail<T>(string mensaje)
    {
        return new ActionResponse<T> { WasSuccess = false, Message = mensaje };
    }
}
