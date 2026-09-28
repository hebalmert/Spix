using Microsoft.EntityFrameworkCore;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.MkDTOs;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementMk;

// v2 persists the configuration written by WPF over LAN. It never connects to RouterOS.
public partial class MkConnectionService
{
    public async Task<ActionResponse<PppoeServerLocalSetupDTO>> GetPppoeLocalSetupAsync(
        Guid serverId, string? serviceName, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return Fallo<PppoeServerLocalSetupDTO>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var server = await _context.Servers.AsNoTracking()
                .Include(x => x.IpNetwork)
                .Include(x => x.PppLocalIpNet)
                .FirstOrDefaultAsync(x => x.ServerId == serverId && x.CorporationId == user.CorporationId);
            if (server == null) return Fallo<PppoeServerLocalSetupDTO>(_localizer[nameof(Resource.Server_Not_Found)]);
            if (server.ControlMk != MikrotikControlType.PPPoE)
                return Fallo<PppoeServerLocalSetupDTO>(_localizer[nameof(Resource.Server_NotPppoe)]);
            if (string.IsNullOrWhiteSpace(server.LanName))
                return Fallo<PppoeServerLocalSetupDTO>(_localizer[nameof(Resource.Server_LanNameRequired)]);
            if (server.PppLocalIpNet?.Ip == null || server.PppLocalIpNet.CorporationId != user.CorporationId)
                return Fallo<PppoeServerLocalSetupDTO>(_localizer[nameof(Resource.Server_PppLocalIpRequired)]);
            if (string.IsNullOrWhiteSpace(server.IpNetwork?.Ip))
                return Fallo<PppoeServerLocalSetupDTO>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);

            return Exito(new PppoeServerLocalSetupDTO
            {
                ServerId = server.ServerId,
                ServerName = server.ServerName,
                ServerIp = server.IpNetwork.Ip,
                Username = server.Usuario,
                Password = server.Clave,
                ApiPort = server.ApiPort,
                LanName = server.LanName,
                LocalIp = server.PppLocalIpNet.Ip,
                ProfileName = Nombrar(server.PppProfileName, $"spix-{server.ServerName}"),
                ServiceName = Nombrar(serviceName ?? server.PppServiceName, $"spix-{server.ServerName}"),
                IsProvisioned = !string.IsNullOrWhiteSpace(server.PppServerMkId)
            });
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<PppoeServerLocalSetupDTO>(ex);
        }
    }

    //Prepara el BORRADO: entrega como llegar al equipo y los dos .id a quitar.
    //Aqui se hace la guarda dura, antes de que el escritorio toque nada: con contratos
    //PPPoE colgados de este servidor no se borra, porque sus secrets usan este perfil.
    public async Task<ActionResponse<PppoeServerLocalRemoveDTO>> GetPppoeLocalRemoveAsync(
        Guid serverId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return Fallo<PppoeServerLocalRemoveDTO>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var server = await _context.Servers.AsNoTracking()
                .Include(x => x.IpNetwork)
                .FirstOrDefaultAsync(x => x.ServerId == serverId && x.CorporationId == user.CorporationId);

            if (server == null) return Fallo<PppoeServerLocalRemoveDTO>(_localizer[nameof(Resource.Server_Not_Found)]);

            if (string.IsNullOrWhiteSpace(server.PppServerMkId) &&
                string.IsNullOrWhiteSpace(server.PppProfileMkId))
            {
                return Fallo<PppoeServerLocalRemoveDTO>(_localizer[nameof(Resource.Server_PppoeNotCreated)]);
            }

            var conContratos = await _context.ContractPppoes.AnyAsync(x => x.ServerId == serverId);
            if (conContratos) return Fallo<PppoeServerLocalRemoveDTO>(_localizer[nameof(Resource.Server_PppoeHasContracts)]);

            if (string.IsNullOrWhiteSpace(server.IpNetwork?.Ip))
                return Fallo<PppoeServerLocalRemoveDTO>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);

            return Exito(new PppoeServerLocalRemoveDTO
            {
                ServerId = server.ServerId,
                ServerIp = server.IpNetwork.Ip,
                Username = server.Usuario,
                Password = server.Clave,
                ApiPort = server.ApiPort,
                ProfileMikrotikId = server.PppProfileMkId ?? string.Empty,
                ServerMikrotikId = server.PppServerMkId ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<PppoeServerLocalRemoveDTO>(ex);
        }
    }

    //Limpia el espejo DESPUES de que el escritorio ya borro en el equipo. Se vuelve a
    //comprobar lo de los contratos: entre el preparar y el guardar pudo entrar uno.
    public async Task<ActionResponse<bool>> ClearPppoeLocalAsync(Guid serverId, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return await FalloRollbackAsync(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var server = await _context.Servers
                .FirstOrDefaultAsync(x => x.ServerId == serverId && x.CorporationId == user.CorporationId);
            if (server == null) return await FalloRollbackAsync(_localizer[nameof(Resource.Server_Not_Found)]);

            var conContratos = await _context.ContractPppoes.AnyAsync(x => x.ServerId == serverId);
            if (conContratos) return await FalloRollbackAsync(_localizer[nameof(Resource.Server_PppoeHasContracts)]);

            //Mismo resultado que el borrado de Blazor: el bloque queda en blanco
            server.PppProfileName = null;
            server.PppProfileMkId = null;
            server.PppServiceName = null;
            server.PppServerMkId = null;
            server.PppLocalIpNetId = null;

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();
            return Exito(true);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    public async Task<ActionResponse<bool>> SavePppoeLocalAsync(PppoeServerLocalSaveDTO datos, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return await FalloRollbackAsync(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var server = await _context.Servers
                .Include(x => x.PppLocalIpNet)
                .FirstOrDefaultAsync(x => x.ServerId == datos.ServerId && x.CorporationId == user.CorporationId);
            if (server == null) return await FalloRollbackAsync(_localizer[nameof(Resource.Server_Not_Found)]);
            if (server.ControlMk != MikrotikControlType.PPPoE)
                return await FalloRollbackAsync(_localizer[nameof(Resource.Server_NotPppoe)]);
            if (!string.IsNullOrWhiteSpace(server.PppServerMkId) || !string.IsNullOrWhiteSpace(server.PppProfileMkId))
                return await FalloRollbackAsync(_localizer[nameof(Resource.Server_PppoeAlreadyExists)]);
            if (string.IsNullOrWhiteSpace(server.LanName) ||
                server.PppLocalIpNet?.Ip == null || server.PppLocalIpNet.CorporationId != user.CorporationId)
                return await FalloRollbackAsync(_localizer[nameof(Resource.Pppoe_ServerNotReady)]);

            var expectedProfile = Nombrar(server.PppProfileName, $"spix-{server.ServerName}");
            if (datos.ProfileName != expectedProfile ||
                string.IsNullOrWhiteSpace(datos.ServiceName) || datos.ServiceName.Length > 50 ||
                datos.ServiceName != Nombrar(datos.ServiceName, datos.ServiceName) ||
                string.IsNullOrWhiteSpace(datos.ProfileMikrotikId) || datos.ProfileMikrotikId.Length > 15 ||
                string.IsNullOrWhiteSpace(datos.ServerMikrotikId) || datos.ServerMikrotikId.Length > 15)
            {
                return await FalloRollbackAsync(_localizer[nameof(Resource.Generic_InvalidModel)]);
            }

            server.PppProfileName = datos.ProfileName;
            server.PppProfileMkId = datos.ProfileMikrotikId;
            server.PppServiceName = datos.ServiceName;
            server.PppServerMkId = datos.ServerMikrotikId;
            //La IP local no se marca: ver CreatePppoeServerAsync

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();
            return Exito(true);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }
}
