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

    public async Task<ActionResponse<ContractBind>> GetAsync(Guid id)
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
            var modelo = await _context.ContractBinds
                .Include(x => x.Server)
                .Include(x => x.IpNet)
                .Include(x => x.CargueDetail)
                .Include(x => x.HotSpotType)
                .FirstOrDefaultAsync(c => c.ContractClientId == id);

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

            var conServer = await _context.Servers
                .AsNoTracking()
                .Include(x => x.IpNetwork).FirstOrDefaultAsync(x => x.ServerId == modelo.ServerId);
            var conIpClient = await _context.IpNets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IpNetId == modelo.IpNetId);
            var conCliente = await _context.ContractClients
                .AsNoTracking()
                .Include(x => x.Client).FirstOrDefaultAsync(x => x.ContractClientId == modelo!.ContractClientId);
            var conMac = await _context.CargueDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CargueDetailId == modelo.CargueDetailId);
            var typehot = await _context.HotSpotTypes.FindAsync(modelo.HotSpotTypeId);

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

    public async Task<ActionResponse<ContractBind>> UpdateAsync(ContractBind modelo)
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
            var data = await _context.ContractBinds.FindAsync(modelo.ContractBindId);
            if (data == null)
            {
                return new ActionResponse<ContractBind>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            data.ContractClientId = modelo.ContractClientId;
            data.ServerId = modelo.ServerId;
            data.IpNetId = modelo.IpNetId;
            data.CargueDetailId = modelo.CargueDetailId;
            data.HotSpotTypeId = modelo.HotSpotTypeId;
            data.ServerName = modelo.ServerName;
            data.IpServer = modelo.IpServer;
            data.IpCliente = modelo.IpCliente;
            data.MacCliente = modelo.MacCliente;
            data.MikrotikId = modelo.MikrotikId;

            _context.ContractBinds.Update(data);
            await _transactionManager.SaveChangesAsync();


            var conServer = await _context.Servers
                .AsNoTracking()
                .Include(x => x.IpNetwork).FirstOrDefaultAsync(x => x.ServerId == modelo.ServerId);
            var conIpClient = await _context.IpNets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IpNetId == modelo.IpNetId);
            var conCliente = await _context.ContractClients
                .AsNoTracking()
                .Include(x => x.Client).FirstOrDefaultAsync(x => x.ContractClientId == modelo!.ContractClientId);
            var conMac = await _context.CargueDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CargueDetailId == modelo.CargueDetailId);
            var typehot = await _context.HotSpotTypes.FindAsync(modelo.HotSpotTypeId);

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
                macCliente = $"{conMac!.MacWlan}",
                idIpBinding = modelo.MikrotikId
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

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var dataRemove = await _context.ContractBinds.FindAsync(id);
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
