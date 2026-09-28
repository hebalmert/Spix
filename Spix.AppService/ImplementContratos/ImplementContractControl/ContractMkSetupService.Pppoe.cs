using Microsoft.EntityFrameworkCore;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementContratos.ImplementContractControl;

// The desktop writes RouterOS over the local network; these methods only validate and
// persist the matching database record. The Blazor/v1 RouterOS flow stays unchanged.
public partial class ContractMkSetupService
{
    public async Task<ActionResponse<ContractPppoeLocalSetupDTO>> GetPppoeSetupAsync(Guid contractClientId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return Fail<ContractPppoeLocalSetupDTO>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var contract = await _context.ContractClients.AsNoTracking()
                .Include(x => x.Client)
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId && x.CorporationId == user.CorporationId);
            if (contract == null) return Fail<ContractPppoeLocalSetupDTO>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var assignment = await _context.ContractServers.AsNoTracking()
                .Include(x => x.Server!).ThenInclude(x => x.IpNetwork)
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId);
            var contractIp = await _context.ContractIps.AsNoTracking()
                .Include(x => x.IpNet)
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId);

            var server = assignment?.Server;
            if (server == null || server.CorporationId != user.CorporationId ||
                server.ControlMk != MikrotikControlType.PPPoE ||
                string.IsNullOrWhiteSpace(server.PppProfileName) ||
                string.IsNullOrWhiteSpace(server.IpNetwork?.Ip) ||
                string.IsNullOrWhiteSpace(contractIp?.IpNet?.Ip) ||
                contractIp.IpNet.CorporationId != user.CorporationId)
            {
                return Fail<ContractPppoeLocalSetupDTO>(_localizer[nameof(Resource.Pppoe_ServerNotReady)]);
            }

            var current = await _context.ContractPppoes.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId);

            return Ok(new ContractPppoeLocalSetupDTO
            {
                ContractClientId = contractClientId,
                ServerId = server.ServerId,
                IpNetId = contractIp.IpNetId,
                ServerName = server.ServerName,
                ServerIp = server.IpNetwork.Ip,
                ServerUser = server.Usuario,
                ServerPassword = server.Clave,
                ApiPort = server.ApiPort,
                ClientIp = contractIp.IpNet.Ip,
                ProfileName = server.PppProfileName,
                ClientName = $"{contract.Client?.FirstName} {contract.Client?.LastName} - ({contract.ControlContrato})".Trim(),
                CredentialId = current?.ContractPppoeId,
                MikrotikId = current?.MikrotikId,
                CurrentUsername = current?.Usuario,
                CurrentPassword = current?.Clave,
                AccessState = current?.PppoeAccessState ?? PppoeAccessState.Activo
            });
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractPppoeLocalSetupDTO>(ex);
        }
    }

    public async Task<ActionResponse<ContractPppoe>> SavePppoeAsync(ContractPppoeLocalSaveDTO datos, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return await PppoeRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var contract = await _context.ContractClients.AsNoTracking()
                .Include(x => x.Client)
                .FirstOrDefaultAsync(x => x.ContractClientId == datos.ContractClientId && x.CorporationId == user.CorporationId);
            if (contract == null) return await PppoeRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var assignment = await _context.ContractServers.AsNoTracking()
                .Include(x => x.Server!).ThenInclude(x => x.IpNetwork)
                .FirstOrDefaultAsync(x => x.ContractClientId == datos.ContractClientId);
            var contractIp = await _context.ContractIps.AsNoTracking().Include(x => x.IpNet)
                .FirstOrDefaultAsync(x => x.ContractClientId == datos.ContractClientId);
            var server = assignment?.Server;
            if (server == null || server.CorporationId != user.CorporationId ||
                server.ControlMk != MikrotikControlType.PPPoE ||
                string.IsNullOrWhiteSpace(server.PppProfileName) ||
                string.IsNullOrWhiteSpace(server.IpNetwork?.Ip) ||
                string.IsNullOrWhiteSpace(contractIp?.IpNet?.Ip) ||
                contractIp.IpNet.CorporationId != user.CorporationId)
            {
                return await PppoeRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Pppoe_ServerNotReady)]);
            }

            var normalizedUser = datos.Username.Trim().ToLowerInvariant();
            if (normalizedUser.Length == 0 || normalizedUser.Length > 50 ||
                string.IsNullOrWhiteSpace(datos.Password) || datos.Password.Length > 50 ||
                string.IsNullOrWhiteSpace(datos.MikrotikId) || datos.MikrotikId.Length > 15)
            {
                return await PppoeRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Generic_InvalidModel)]);
            }

            var repeated = await _context.ContractPppoes.AnyAsync(x => x.ServerId == server.ServerId &&
                x.Usuario == normalizedUser && x.ContractPppoeId != datos.CredentialId);
            if (repeated) return await PppoeRollbackAsync<ContractPppoe>(_localizer["Pppoe_UserRepeated", normalizedUser]);

            var current = await _context.ContractPppoes
                .FirstOrDefaultAsync(x => x.ContractClientId == datos.ContractClientId);

            if (datos.CredentialId.HasValue)
            {
                if (current == null || current.ContractPppoeId != datos.CredentialId ||
                    current.ServerId != server.ServerId || current.IpNetId != contractIp.IpNetId ||
                    current.MikrotikId != datos.MikrotikId)
                {
                    return await PppoeRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Generic_IdNotFound)]);
                }
            }
            else
            {
                if (current != null) return await PppoeRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Pppoe_AlreadyExists)]);

                current = new ContractPppoe
                {
                    ContractClientId = datos.ContractClientId,
                    ServerId = server.ServerId,
                    IpNetId = contractIp.IpNetId,
                    MikrotikId = datos.MikrotikId,
                    PppoeAccessState = PppoeAccessState.Activo
                };
                _context.ContractPppoes.Add(current);
            }

            current.Usuario = normalizedUser;
            current.Clave = datos.Password;
            current.ServerName = server.ServerName;
            current.IpServer = server.IpNetwork.Ip;
            current.IpCliente = contractIp.IpNet.Ip;
            current.ProfileName = server.PppProfileName;

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();
            return Ok(current);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractPppoe>(ex);
        }
    }

    public async Task<ActionResponse<bool>> RemovePppoeAsync(Guid credentialId, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return await PppoeRollbackAsync<bool>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.ContractPppoes.FirstOrDefaultAsync(x =>
                x.ContractPppoeId == credentialId && x.ContractClient!.CorporationId == user.CorporationId);
            if (current == null) return await PppoeRollbackAsync<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            _context.ContractPppoes.Remove(current);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();
            return Ok(true);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    private async Task<ActionResponse<T>> PppoeRollbackAsync<T>(string message)
    {
        await _transactionManager.RollbackTransactionAsync();
        return Fail<T>(message);
    }
}
