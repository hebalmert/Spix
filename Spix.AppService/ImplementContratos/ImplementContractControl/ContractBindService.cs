using Spix.DomainLogic.EnumTypes;
using Spix.AppService.ImplementContratos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;
using Spix.xNetwork.MkHelper;

namespace Spix.AppService.ImplementEntitiesNet;

public class ContractBindService : IContractBindService
{
    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;

    public ContractBindService(DataContext context, ITransactionManager transactionManager,
        IUserHelper userHelper, IStringLocalizer localizer, HttpErrorHandler httpErrorHandler)
    {
        _context = context;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _localizer = localizer;
        _httpErrorHandler = httpErrorHandler;
    }

    //Lo que muestra Control de Contratos. Solo contratos de la corporacion del usuario,
    //y sin la clave del equipo: la pantalla no la usa y las operaciones con el MikroTik
    //la leen directo de la base.
    public async Task<ActionResponse<ContractBind>> GetAsync(Guid id, string username)
    {
        if (id == Guid.Empty)
        {
            return new ActionResponse<ContractBind>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<ContractBind>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var modelo = await _context.ContractBinds.AsNoTracking()
                .Include(x => x.Server)
                .Include(x => x.IpNet)
                .Include(x => x.CargueDetail)
                .Include(x => x.HotSpotType)
                .FirstOrDefaultAsync(c => c.ContractClientId == id && c.ContractClient!.CorporationId == user.CorporationId);

            if (modelo?.Server != null)
            {
                modelo.Server.Clave = string.Empty;
            }

            return new ActionResponse<ContractBind>
            {
                WasSuccess = true,
                Result = modelo ?? new()
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractBind>(ex);
        }
    }

    public async Task<ActionResponse<ContractBind>> AddAsync(ContractBind modelo, string username)
    {
        if (modelo.ContractClientId == Guid.Empty ||
            modelo.ServerId == Guid.Empty ||
            modelo.IpNetId == Guid.Empty ||
            modelo.CargueDetailId == Guid.Empty ||
            modelo.HotSpotTypeId == 0)
        {
            return new ActionResponse<ContractBind>
            {
                WasSuccess = false,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<ContractBind>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var exists = await _context.ContractBinds.AnyAsync(x => x.ContractClientId == modelo.ContractClientId);
            if (exists)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractBind>
                {
                    WasSuccess = false,
                    Message = "Ya existe un IpBinding de Acceso para este contrato."
                };
            }

            //El equipo, la IP y el contrato tienen que ser de la corporacion del usuario, y
            //se comprueba ANTES de escribirle al MikroTik: si no, con los ids de otra empresa
            //se le escribe un binding a su router y solo despues falla algo.
            var conServer = await _context.Servers
                .AsNoTracking()
                .Include(x => x.IpNetwork)
                .FirstOrDefaultAsync(x => x.ServerId == modelo.ServerId &&
                                          x.CorporationId == user.CorporationId);
            var conIpClient = await _context.IpNets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IpNetId == modelo.IpNetId &&
                                          x.CorporationId == user.CorporationId);
            var conCliente = await _context.ContractClients
                .AsNoTracking()
                .Include(x => x.Client)
                .FirstOrDefaultAsync(x => x.ContractClientId == modelo!.ContractClientId &&
                                          x.CorporationId == user.CorporationId);
            var conMac = await _context.CargueDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CargueDetailId == modelo.CargueDetailId &&
                                          x.CorporationId == user.CorporationId);
            var typehot = await _context.HotSpotTypes.FindAsync(modelo.HotSpotTypeId);

            //Si algo no es de su corporacion se corta aca, SIN haber tocado el equipo
            //Y tiene que ser EL equipo y LA IP que el contrato tiene asignados, no otros de
            //la misma empresa: el sistema resuelve el tipo de control por ContractServer, asi
            //que un binding en otro equipo quedaria fuera de todos los lotes.
            var equipoDelContrato = await _context.ContractServers
                .AsNoTracking()
                .AnyAsync(x => x.ContractClientId == modelo.ContractClientId &&
                               x.ServerId == modelo.ServerId);

            var ipDelContrato = await _context.ContractIps
                .AsNoTracking()
                .AnyAsync(x => x.ContractClientId == modelo.ContractClientId &&
                               x.IpNetId == modelo.IpNetId);

            if (!equipoDelContrato || !ipDelContrato)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractBind>
                {
                    WasSuccess = false,
                    Message = _localizer["Contract_ServerMismatch"]
                };
            }

            if (conServer == null || conIpClient == null || conCliente == null || conMac == null || typehot == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractBind>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            ////////////////////////////////////////////////////////////
            var dato = new
            {
                nameserver = conServer!.ServerName,
                ipservidor = conServer.IpNetwork!.Ip,
                us = conServer.Usuario,
                pss = conServer.Clave,
                puerto = conServer.ApiPort,
                ipcliente = conIpClient!.Ip,
                nomCliente = $"{conCliente!.Client!.FirstName} {conCliente!.Client!.LastName} - ({conCliente.ControlContrato})",
                macCliente = $"{conMac!.MacWlan}"
            };

            MK mikrotik = new MK(dato.ipservidor!, dato.puerto);
            if (!mikrotik.Login(dato.us, dato.pss))
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractBind>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Mikrotik_Connection_Error)]
                };
            }

            mikrotik.Send("/ip/hotspot/ip-binding/add");
            mikrotik.Send("=address=" + dato.ipcliente);
            mikrotik.Send("=to-address=" + dato.ipcliente);
            mikrotik.Send("=comment=" + dato.nomCliente);
            mikrotik.Send("=mac-address=" + dato.macCliente);
            mikrotik.Send("=server=" + "all");
            mikrotik.Send("=type=" + typehot!.TypeName);
            mikrotik.Send("/ip/hotspot/ip-binding/print", true);

            int total = 0;
            int rest = 0;
            string idmk;
            string mikrotiIndex = string.Empty;

            foreach (var item in mikrotik.Read())
            {
                idmk = item;
                total = idmk.Length;
                rest = total - 10;
                mikrotiIndex = idmk.Substring(10, rest);
            }

            mikrotik.Close();
            ///////////////////////////////////////////////////////////////////////////////////
            /// 

            modelo.MikrotikId = mikrotiIndex;

            _context.ContractBinds.Add(modelo);

            await ContractAuditLog.AddAsync(_context, modelo.ContractClientId, ContractEventType.BindCreated,
                modelo.MikrotikId, $"{user.FirstName} {user.LastName}".Trim(),
                Guid.TryParse(user.Id, out var auditUserId) ? auditUserId : null);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractBind>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractBind>(ex);
        }
    }

    public async Task<ActionResponse<ContractBind>> UpdateAsync(ContractBind modelo, string username)
    {
        if (modelo.ContractBindId == Guid.Empty ||
            modelo.ContractClientId == Guid.Empty ||
            modelo.ServerId == Guid.Empty ||
            modelo.IpNetId == Guid.Empty ||
            modelo.CargueDetailId == Guid.Empty ||
            modelo.HotSpotTypeId == 0)
        {
            return new ActionResponse<ContractBind>
            {
                WasSuccess = false,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<ContractBind>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            //Por id SOLO no alcanza: el binding tiene que ser de un contrato de SU corporacion
            var data = await _context.ContractBinds
                .FirstOrDefaultAsync(x => x.ContractBindId == modelo.ContractBindId &&
                                          x.ContractClient!.CorporationId == user.CorporationId);

            if (data == null)
            {
                return new ActionResponse<ContractBind>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            //El equipo y el contrato NO se cambian en una edicion.
            //
            //El MikrotikId guardado es el id de una fila DE ESE router. Si se aceptara otro
            //ServerId, el set de mas abajo se ejecutaria en el router nuevo contra un id que
            //alli le pertenece a otro registro, y le reescribiria el binding a otro cliente.
            //Mudar un binding de equipo es borrarlo y volverlo a crear.
            if (modelo.ServerId != data.ServerId || modelo.ContractClientId != data.ContractClientId)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractBind>
                {
                    WasSuccess = false,
                    Message = _localizer["ContractBind_ServerCannotChange"]
                };
            }

            //La IP y la MAC pueden cambiar, pero tienen que ser las que el contrato tiene
            //asignadas. La IP viaja al router como address y to-address del binding: si se
            //aceptara cualquier IP de la empresa, se le escribiria en el equipo la IP de otro
            //cliente. Es la misma comprobacion que hace el alta.
            var ipDelContrato = await _context.ContractIps
                .AsNoTracking()
                .AnyAsync(x => x.ContractClientId == data.ContractClientId &&
                               x.IpNetId == modelo.IpNetId);

            var macDelContrato = await _context.ContractMacs
                .AsNoTracking()
                .AnyAsync(x => x.ContractClientId == data.ContractClientId &&
                               x.CargueDetailId == modelo.CargueDetailId);

            if (!ipDelContrato || !macDelContrato)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractBind>
                {
                    WasSuccess = false,
                    Message = _localizer["Contract_ServerMismatch"]
                };
            }

            data.IpNetId = modelo.IpNetId;
            data.CargueDetailId = modelo.CargueDetailId;
            data.HotSpotTypeId = modelo.HotSpotTypeId;
            data.ServerName = modelo.ServerName;
            data.IpServer = modelo.IpServer;
            data.IpCliente = modelo.IpCliente;
            data.MacCliente = modelo.MacCliente;

            //El MikrotikId tampoco se acepta del modelo: lo escribio Spix al crear.

            _context.ContractBinds.Update(data);
            await _transactionManager.SaveChangesAsync();


            //Todo dentro de la corporacion del usuario, y comprobado antes de tocar el equipo
            var conServer = await _context.Servers
                .AsNoTracking()
                .Include(x => x.IpNetwork)
                .FirstOrDefaultAsync(x => x.ServerId == modelo.ServerId &&
                                          x.CorporationId == user.CorporationId);
            var conIpClient = await _context.IpNets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IpNetId == modelo.IpNetId &&
                                          x.CorporationId == user.CorporationId);
            var conCliente = await _context.ContractClients
                .AsNoTracking()
                .Include(x => x.Client)
                .FirstOrDefaultAsync(x => x.ContractClientId == modelo!.ContractClientId &&
                                          x.CorporationId == user.CorporationId);
            var conMac = await _context.CargueDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CargueDetailId == modelo.CargueDetailId &&
                                          x.CorporationId == user.CorporationId);
            var typehot = await _context.HotSpotTypes.FindAsync(modelo.HotSpotTypeId);

            if (conServer == null || conIpClient == null || conCliente == null || conMac == null || typehot == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractBind>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            ////////////////////////////////////////////////////////////
            var dato = new
            {
                nameserver = conServer.ServerName,
                ipservidor = conServer.IpNetwork!.Ip,
                us = conServer.Usuario,
                pss = conServer.Clave,
                puerto = conServer.ApiPort,
                ipcliente = conIpClient.Ip,
                nomCliente = $"{conCliente.Client!.FirstName} {conCliente.Client!.LastName} - ({conCliente.ControlContrato})",
                macCliente = $"{conMac.MacWlan}",

                //El id guardado, no el del modelo
                idIpBinding = data.MikrotikId
            };

            ////////////////////////////////////////////////////////////
            MK mikrotik = new MK(dato.ipservidor!, dato.puerto);
            if (!mikrotik.Login(dato.us, dato.pss))
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractBind>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Mikrotik_Connection_Error)]
                };
            }

            mikrotik.Send("/ip/hotspot/ip-binding/set");
            mikrotik.Send("=.id=" + dato.idIpBinding);
            mikrotik.Send("=address=" + dato.ipcliente);
            mikrotik.Send("=to-address=" + dato.ipcliente);
            mikrotik.Send("=comment=" + dato.nomCliente);
            mikrotik.Send("=mac-address=" + dato.macCliente);
            mikrotik.Send("=server=" + "all");
            mikrotik.Send("=type=" + typehot!.TypeName);
            mikrotik.Send("/ip/hotspot/ip-binding/print", true);

            int total = 0;
            int rest = 0;
            string idmk;
            string mikrotiIndex = string.Empty;

            foreach (var item in mikrotik.Read())
            {
                idmk = item;
                total = idmk.Length;
                rest = total - 10;
            }

            mikrotik.Close();
            ///////////////////////////////////////////////////////////////////////////////////
            /// 

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractBind>
            {
                WasSuccess = true,
                Result = data
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractBind>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            //Por id SOLO no alcanza: la pieza tiene que ser de un contrato de SU corporacion.
            //Con FindAsync(id) cualquiera que conociera el id borraba la pieza de otra empresa.
            var dataRemove = await _context.ContractBinds
                .FirstOrDefaultAsync(x => x.ContractBindId == id &&
                                          x.ContractClient!.CorporationId == user.CorporationId);
            if (dataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.ContractBinds.Remove(dataRemove);
            await _transactionManager.SaveChangesAsync();

            var conServer = await _context.Servers
                .AsNoTracking()
                .Include(x => x.IpNetwork).FirstOrDefaultAsync(x => x.ServerId == dataRemove.ServerId);
            var dato = new
            {
                ipservidor = conServer!.IpNetwork!.Ip,
                us = conServer!.Usuario,
                pss = conServer!.Clave,
                puerto = conServer.ApiPort,
            };
            //Se hace con conexion a la Mikroti y se deja abierto
            ////////////////////////////////////////////////////////////
            MK mikrotik = new MK(dato.ipservidor!, dato.puerto);
            if (!mikrotik.Login(dato.us, dato.pss))
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Mikrotik_Connection_Error)]
                };
            }

            mikrotik.Send("/ip/hotspot/ip-binding/remove");
            mikrotik.Send("=.id=" + dataRemove.MikrotikId, true);

            int total = 0;
            int rest = 0;
            string idmk;

            foreach (var item in mikrotik.Read())
            {
                idmk = item;
                total = idmk.Length;
                rest = total - 10;
            }

            mikrotik.Close();
            ///////////////////////////////////////////////////////////////////////////////////
            /// 

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }
}
